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
public sealed class WebRtcFromGameplayToGameplayCommunicator :
    Node, IShardServerConnector, IFromGameplayPacketSender, IWebrtcPacketReceiver, INetworkDebugger, IConnectionNotifier
{
    private const double PingInterval = 1;
    
    // Web export has a bug that seemingly corrupts memory when receiving a large packet.
    // Chunking is used as a workaround.
    // TODO report the bug to Godot
    private const int MaxChunkSize = 32000;
    
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
    private readonly IChunkCollector _chunkCollector;
    private readonly ISideDetector _sideDetector;
    
    public long BytesSent { get; private set; }
    public long BytesReceived { get; private set; }

    public WebRtcFromGameplayToGameplayCommunicator
    (
        IFromGameplayToCampaignServerPacketSender campaignServerPacketSender,
        IPacketHandler packetHandler,
        IPacketSerializer packetSerializer, 
        IChunkCollector chunkCollector,
        ISideDetector sideDetector
    )
    {
        _campaignServerPacketSender = campaignServerPacketSender;
        _packetHandler = packetHandler;
        _packetSerializer = packetSerializer;
        _chunkCollector = chunkCollector;
        _sideDetector = sideDetector;

        Name = nameof(WebRtcFromGameplayToGameplayCommunicator);
        ProcessPriority = (int)ProcessPriorityEnum.Communicator;
        PauseMode = PauseModeEnum.Process;
    }

    public override void _PhysicsProcess(float delta)
    {
        _isPhysicsProcess = true;
        
        // Poll() is duplicated in _PhysicsProcess because it runs before _Process, so any packets received during
        // _Process would be delayed to the next physics frame otherwise.
        Poll(delta);
        
        if (_sideDetector.IsServer)
        {
            while (_packetQueue.Count > 0)
            {
                (Packet packet, Guid senderId) = _packetQueue.Dequeue();
                HandlePacket(packet, senderId);
            }
        }
        
        _isPhysicsProcess = false;
    }

    public override void _Process(float delta)
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

            WebRTCPeerConnection.ConnectionState state = peerConnectionAndChannels.Connection.GetConnectionState();
            if (state == WebRTCPeerConnection.ConnectionState.Closed)
            {
                OnPeerDisconnected(peerId);
                continue;
            }
            if (state == WebRTCPeerConnection.ConnectionState.Connected && _connectedPeerIds.Add(peerId))
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
        if (_sideDetector.IsServer) return;
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
    
    private void DeserializeAndHandlePackets(WebRTCDataChannel channel, Guid senderId)
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
        catch (BadSerializedDataException e)
        {
            if (_sideDetector.IsServer)
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
                byte[]? restoredPacketBytes = _chunkCollector.AddChunk(chunkPacket, senderId);
                if (restoredPacketBytes != null)
                    DeserializeAndHandlePacket(restoredPacketBytes, senderId);
                return;
            }
            
            // Server defers packet handling to _PhysicsProcess to ensure that all game logic is executed in it only
            if (_sideDetector.IsServer && !_isPhysicsProcess)
                _packetQueue.Enqueue((packet, senderId));
            else
                await _packetHandler.HandleAsync(packet, senderId);
        }
        catch (BadPacketException e)
        {
            if (_sideDetector.IsServer)
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
        if (_sideDetector.IsServer) throw new InvalidOperationException();
        
        WebRTCPeerConnection connection = CreateConnection(id);
        connection.CreateOffer();
    }
    
    private WebRTCPeerConnection CreateConnection(Guid peerId)
    {
        if (_peerConnectionsAndChannels.TryGetValue(peerId, out PeerConnectionAndChannels existing))
        {
            existing.Connection.Close();
            OnPeerDisconnected(peerId);
        }

        var connection = new WebRTCPeerConnection();
        byte[] peerIdBytes = peerId.ToByteArray();
        connection.Connect("session_description_created", this, nameof(OnSessionDescriptionCreated), [peerIdBytes]);
        connection.Connect("ice_candidate_created", this, nameof(OnIceCandidateCreated), [peerIdBytes]);
        
        WebRTCDataChannel reliableChannel = connection.CreateDataChannel("reliable", new GdDictionary
        {
            ["negotiated"] = true,
            ["id"] = 0,
        });
        WebRTCDataChannel unreliableChannel = connection.CreateDataChannel("unreliable", new GdDictionary
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
    
    private void OnSessionDescriptionCreated(string type, string sdp, byte[] peerIdBytes)
    {
        var peerId = new Guid(peerIdBytes);
        _peerConnectionsAndChannels[peerId].Connection.SetLocalDescription(type, sdp);
        _campaignServerPacketSender.SendPacket(new WebrtcSdpPacket { Sdp = sdp, PeerId = peerId } );
    }
    
    private void OnIceCandidateCreated(string media, int index, String name, byte[] peerIdBytes)
    {
        var peerId = new Guid(peerIdBytes);
        _campaignServerPacketSender.SendPacket(new WebrtcIceCandidatePacket
        {
            Media = media,
            Index = index,
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
        Func<PeerConnectionAndChannels, WebRTCDataChannel> channelSelector
    )
    {
        byte[] bytes = _packetSerializer.Serialize(packet);
        if (bytes.Length <= MaxChunkSize)
        {
            foreach (Guid receiverId in receiverIds)
                SendWithoutChunking(bytes, receiverId, channelSelector);
        }
        else
        {
            byte[][] chunks = SplitIntoChunks(bytes);
            foreach (Guid receiverId in receiverIds)
            foreach (byte[] chunk in chunks)
                SendWithoutChunking(chunk, receiverId, channelSelector);
        }
    }

    private void SendWithoutChunking
    (
        byte[] bytes,
        Guid receiverId,
        Func<PeerConnectionAndChannels, WebRTCDataChannel> channelSelector
    )
    {
        if (!_peerConnectionsAndChannels.TryGetValue(receiverId, out var connectionAndChannels)) return;
        WebRTCDataChannel channel = channelSelector(connectionAndChannels);
        if (channel.GetReadyState() != WebRTCDataChannel.ChannelState.Open) return;
        channel.PutPacket(bytes);
        BytesSent += bytes.Length;
    }
    
    private byte[][] SplitIntoChunks(Span<byte> bytes)
    {
        var groupId = Guid.NewGuid();
        var chunks = new byte[Maths.CeilToInt((double)bytes.Length / MaxChunkSize)][];
        for (int i = 0; i < chunks.Length; i++)
        {
            bool isLast = i == chunks.Length - 1;
            int start = i * MaxChunkSize;
            int end = isLast ? bytes.Length : start + MaxChunkSize;
            var chunkPacket = new ChunkPacket
            {
                GroupId = groupId,
                Index = i,
                IsLast = isLast,
                Bytes = bytes[start..end].ToArray()
            };
            chunks[i] = _packetSerializer.Serialize(chunkPacket);
        }
        return chunks;
    }
    
    public void ReceiveWebrtcSdpPacket(WebrtcSdpPacket packet)
    {
        string type = _sideDetector.IsServer ? "offer" : "answer";
        WebRTCPeerConnection? connection = _sideDetector.IsServer ? CreateConnection(packet.PeerId) :
            _peerConnectionsAndChannels.GetOrDefault(packet.PeerId)?.Connection;
        connection?.SetRemoteDescription(type, packet.Sdp);
    }
    
    public void ReceiveWebrtcIceCandidatePacket(WebrtcIceCandidatePacket packet)
    {
        WebRTCPeerConnection? connection = _peerConnectionsAndChannels.GetOrDefault(packet.PeerId)?.Connection;
        connection?.AddIceCandidate(packet.Media, packet.Index, packet.Name);
    }

    public double? Ping(Guid peerId) =>
        _ping.TryGetValue(peerId, out var tuple) ? tuple.ResponseTime : null;
    
    private record PeerConnectionAndChannels
    (
        WebRTCPeerConnection Connection,
        WebRTCDataChannel ReliableChannel,
        WebRTCDataChannel UnreliableChannel
    );
}
