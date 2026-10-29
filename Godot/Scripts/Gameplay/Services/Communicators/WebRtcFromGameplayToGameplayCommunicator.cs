using Soteo.Core.Dto.Options;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Enums;
using Soteo.Core.Exceptions;
using Soteo.Core.Interfaces;
using Soteo.Main.Gameplay.Interfaces;
using Soteo.Util;

namespace Soteo.Main.Gameplay.Services.Communicators;

/// <summary>
/// Communicates between clients and shard servers
/// </summary>
public sealed partial class WebRtcFromGameplayToGameplayCommunicator :
    Node, IShardServerConnector, IFromGameplayPacketSender, IWebrtcPacketReceiver, INetworkDebugger, IConnectionNotifier
{
    private const double PingInterval = 1;

    private double _timeSinceLastPing;
    private Guid? _lastPingId;
    private bool _didPollThisFrame;
    private bool _isPhysicsProcess;

    private readonly Dictionary<Guid, PeerConnectionAndChannels> _peerConnectionsAndChannels = [];
    private readonly Dictionary<Guid, (Guid PingId, double ResponseTime)> _ping = [];
    private readonly HashSet<Guid> _connectedPeerIds = [];

    public event Action<Guid> PeerConnected = delegate { };
    public event Action<Guid> PeerDisconnected = delegate { };

    private readonly Queue<(Packet Packet, Guid SenderId)> _packetQueue = [];

    private readonly IFromGameplayToCampaignServerPacketSender _campaignServerPacketSender;
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketHandler _packetHandler;
    private readonly IPacketChunkCollector _packetChunkCollector;
    private readonly IPacketChunker _packetChunker;
    private readonly SideOptions _sideOptions;

    public long BytesSent { get; private set; }
    public long BytesReceived { get; private set; }

    public WebRtcFromGameplayToGameplayCommunicator
    (
        IFromGameplayToCampaignServerPacketSender campaignServerPacketSender,
        IPacketHandler packetHandler,
        IPacketSerializer packetSerializer,
        IPacketChunkCollector packetChunkCollector,
        IPacketChunker packetChunker,
        SideOptions sideOptions
    )
    {
        _campaignServerPacketSender = campaignServerPacketSender;
        _packetHandler = packetHandler;
        _packetSerializer = packetSerializer;
        _packetChunkCollector = packetChunkCollector;
        _packetChunker = packetChunker;
        _sideOptions = sideOptions;

        Name = nameof(WebRtcFromGameplayToGameplayCommunicator);
        ProcessPriority = (int)ProcessPriorityEnum.Communicator;
        ProcessPhysicsPriority = (int)ProcessPriorityEnum.Communicator;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _PhysicsProcess(double delta)
    {
        _isPhysicsProcess = true;

        // Poll() is duplicated in _PhysicsProcess because it runs before _Process, so any packets received during
        // _Process would be delayed to the next physics frame otherwise.
        Poll(delta);

        if (_sideOptions.Side == Side.ShardServer)
        {
            while (_packetQueue.Count > 0)
            {
                (Packet packet, Guid senderId) = _packetQueue.Dequeue();
                HandlePacket(packet, senderId);
            }
        }

        _isPhysicsProcess = false;
    }

    public override void _Process(double delta)
    {
        Poll(delta);
        ProcessPing(delta);

        _didPollThisFrame = false;
    }

    private void Poll(double delta)
    {
        if (_didPollThisFrame) return;
        _didPollThisFrame = true;

        foreach
        (
            (Guid peerId, PeerConnectionAndChannels peerConnectionAndChannels) in
                _peerConnectionsAndChannels.ToDictionary()
        )
        {
            peerConnectionAndChannels.Connection.Poll();

            WebRtcPeerConnection.ConnectionState state = peerConnectionAndChannels.Connection.GetConnectionState();
            if (state == WebRtcPeerConnection.ConnectionState.Closed)
            {
                OnPeerDisconnected(peerId);
                continue;
            }
            if (state == WebRtcPeerConnection.ConnectionState.Connected && _connectedPeerIds.Add(peerId))
            {
                PeerConnected(peerId);
            }

            DeserializeAndHandlePackets(peerConnectionAndChannels.ReliableChannel, peerId);
            DeserializeAndHandlePackets(peerConnectionAndChannels.UnreliableChannel, peerId);
        }
    }

    private void OnPeerDisconnected(Guid peerId)
    {
        _peerConnectionsAndChannels.Remove(peerId);
        _ping.Remove(peerId);
        if (_connectedPeerIds.Remove(peerId))
            PeerDisconnected(peerId);
    }

    private void ProcessPing(double delta)
    {
        if (_sideOptions.Side == Side.ShardServer) return;
        _timeSinceLastPing += delta;
        if (_timeSinceLastPing >= PingInterval)
        {
            foreach ((Guid peerId, (Guid pingId, _)) in _ping.ToList())
                if (pingId != _lastPingId)
                    _ping.Remove(peerId);
            _timeSinceLastPing = 0;
            _lastPingId = Guid.NewGuid();
            BroadcastUnreliable(new PingPacket { Id = _lastPingId.Value, IsResponse = false });
        }
    }

    private void DeserializeAndHandlePackets(WebRtcDataChannel channel, Guid senderId)
    {
        while (channel.GetAvailablePacketCount() > 0)
        {
            byte[] bytes = channel.GetPacket();
            BytesReceived += bytes.Length;
            DeserializeAndHandlePacket(bytes, senderId);
        }
    }

    private void DeserializeAndHandlePacket(byte[] bytes, Guid senderId)
    {
        Packet? packet = DeserializePacket(bytes, senderId);
        if (packet == null) return;
        HandlePacket(packet, senderId);
    }

    private Packet? DeserializePacket(byte[] bytes, Guid senderId)
    {
        try
        {
            return _packetSerializer.Deserialize(bytes);
        }
        catch (Exception e)
        {
            if (_sideOptions.Side == Side.ShardServer)
            {
                SendReliable(new BadInputPacket { Reason = e.Message }, senderId);
                return null;
            }
            throw;
        }
    }

    private async void HandlePacket(Packet packet, Guid senderId)
    {
        try
        {
            if (packet is PingPacket pingPacket)
            {
                HandlePingPacket(pingPacket, senderId);
                return;
            }
            if (packet is ChunkPacket chunkPacket)
            {
                byte[]? restoredPacketBytes = _packetChunkCollector.AddChunk(chunkPacket, senderId);
                if (restoredPacketBytes != null)
                    DeserializeAndHandlePacket(restoredPacketBytes, senderId);
                return;
            }

            // Server defers packet handling to _PhysicsProcess to ensure that all game logic is executed in it only
            if (_sideOptions.Side == Side.ShardServer && !_isPhysicsProcess)
                _packetQueue.Enqueue((packet, senderId));
            else
                await _packetHandler.HandleAsync(packet, senderId);
        }
        catch (BadPacketException e)
        {
            if (_sideOptions.Side == Side.ShardServer)
                SendReliable(new BadInputPacket { Reason = e.Message }, senderId);
            else
                AsyncExceptionCollector.Collect(e);
        }
        catch (Exception e)
        {
            AsyncExceptionCollector.Collect(e);
        }
    }

    private void HandlePingPacket(PingPacket packet, Guid senderId)
    {
        if (packet.IsResponse)
        {
            if (packet.Id == _lastPingId)
            {
                _ping[senderId] = (packet.Id, _timeSinceLastPing);
            }
        }
        else
        {
            SendUnreliable(packet with { IsResponse = true }, senderId);
        }
    }

    public void ConnectToShardServer(Guid id)
    {
        if (_sideOptions.Side == Side.ShardServer) throw new InvalidOperationException();

        WebRtcPeerConnection connection = CreateConnection(id);
        connection.CreateOffer();
    }

    private WebRtcPeerConnection CreateConnection(Guid peerId)
    {
        if (_peerConnectionsAndChannels.TryGetValue(peerId, out PeerConnectionAndChannels? existing))
        {
            existing.Connection.Close();
            OnPeerDisconnected(peerId);
        }

        var connection = new WebRtcPeerConnection();
        connection.SessionDescriptionCreated += (type, sdp) => OnSessionDescriptionCreated(type, sdp, peerId);
        connection.IceCandidateCreated += (media, index, name) => OnIceCandidateCreated(media, index, name, peerId);

        WebRtcDataChannel reliableChannel = connection.CreateDataChannel("reliable", new GdDictionary
        {
            ["negotiated"] = true,
            ["id"] = 0,
        });
        WebRtcDataChannel unreliableChannel = connection.CreateDataChannel("unreliable", new GdDictionary
        {
            ["negotiated"] = true,
            ["id"] = 1,
            ["maxRetransmits"] = 0,
            ["ordered"] = false
        });

        _peerConnectionsAndChannels[peerId] =
            new PeerConnectionAndChannels(connection, reliableChannel, unreliableChannel);
        return connection;
    }

    private void OnSessionDescriptionCreated(string type, string sdp, Guid peerId)
    {
        _peerConnectionsAndChannels[peerId].Connection.SetLocalDescription(type, sdp);
        _campaignServerPacketSender.SendPacket(new WebrtcSdpPacket { Sdp = sdp, PeerId = peerId } );
    }

    private void OnIceCandidateCreated(string media, long index, String name, Guid peerId)
    {
        _campaignServerPacketSender.SendPacket(new WebrtcIceCandidatePacket
        {
            Media = media,
            Index = (int)index,
            Name = name,
            PeerId = peerId
        });
    }

    public void SendReliable(Packet packet, params IEnumerable<Guid> receiverIds) =>
        SendToMany(packet, receiverIds, it => it.ReliableChannel);

    public void SendUnreliable(Packet packet, params IEnumerable<Guid> receiverIds) =>
        SendToMany(packet, receiverIds, it => it.UnreliableChannel);

    public void BroadcastReliable(Packet packet) =>
        SendToMany(packet, _peerConnectionsAndChannels.Keys, it => it.ReliableChannel);

    public void BroadcastUnreliable(Packet packet) =>
        SendToMany(packet, _peerConnectionsAndChannels.Keys, it => it.UnreliableChannel);

    private void SendToMany
    (
        Packet packet,
        IEnumerable<Guid> receiverIds,
        Func<PeerConnectionAndChannels, WebRtcDataChannel> channelSelector
    )
    {
        // There is an undocumented packet size limit, so large packets are chunked
        byte[][] chunks = _packetChunker.SplitIntoChunks(packet, maxChunkSize: 250000);
        foreach (Guid receiverId in receiverIds)
        {
            PeerConnectionAndChannels? connectionAndChannels;
            if (!_peerConnectionsAndChannels.TryGetValue(receiverId, out connectionAndChannels)) continue;
            WebRtcDataChannel channel = channelSelector(connectionAndChannels);
            if (channel.GetReadyState() != WebRtcDataChannel.ChannelState.Open) continue;
            foreach (byte[] chunk in chunks)
            {
                channel.PutPacket(chunk).ThrowIfError();
                BytesSent += chunk.Length;
            }
        }
    }

    public void ReceiveWebrtcSdpPacket(WebrtcSdpPacket packet)
    {
        string type = _sideOptions.Side == Side.ShardServer ? "offer" : "answer";
        WebRtcPeerConnection? connection = _sideOptions.Side == Side.ShardServer ? CreateConnection(packet.PeerId) :
            _peerConnectionsAndChannels.GetOrDefault(packet.PeerId)?.Connection;
        connection?.SetRemoteDescription(type, packet.Sdp).ThrowIfError();
    }

    public void ReceiveWebrtcIceCandidatePacket(WebrtcIceCandidatePacket packet)
    {
        WebRtcPeerConnection? connection = _peerConnectionsAndChannels.GetOrDefault(packet.PeerId)?.Connection;
        connection?.AddIceCandidate(packet.Media, packet.Index, packet.Name).ThrowIfError();
    }

    public double? Ping(Guid peerId) =>
        _ping.TryGetValue(peerId, out var tuple) ? tuple.ResponseTime : null;

    private record PeerConnectionAndChannels
    (
        WebRtcPeerConnection Connection,
        WebRtcDataChannel ReliableChannel,
        WebRtcDataChannel UnreliableChannel
    );
}
