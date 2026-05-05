using Godot.Collections;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Nodes.Autoloads;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Services.Communicators;

/// <summary>
/// Communicates between clients and shard servers
/// </summary>
public sealed class WebRtcFromGameplayToGameplayCommunicator :
    Node, IPacketSender, IWebrtcPacketReceiver, INetworkDebugger
{
    private record PeerConnectionAndChannels
    (
        WebRTCPeerConnection Connection,
        WebRTCDataChannel ReliableChannel,
        WebRTCDataChannel UnreliableChannel
    );
    
    private const double PingInterval = 1;
    
    // Web export has a bug that seemingly corrupts memory when receiving a large packet.
    // Chunking is used as a workaround.
    // TODO report the bug to Godot
    private const int MaxChunkSize = 32000;
    
    private double _timeSinceLastPing;
    private Guid _lastPingId;
    private bool _didPollThisFrame;
    
    private readonly System.Collections.Generic.Dictionary<Guid, PeerConnectionAndChannels>
        _peerConnectionsAndChannels = [];
    private readonly System.Collections.Generic.Dictionary<Guid, (Guid PingId, double ResponseTime)> _ping = [];
    
    private readonly Queue<(Packet Packet, Guid SenderId)> _packetQueue = [];
    
    private readonly ICampaignServerCommunicator _campaignServerCommunicator;
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketHandler _packetHandler;
    private readonly IChunkCollector _chunkCollector;
    
    public long BytesSent { get; private set; }
    public long BytesReceived { get; private set; }

    public WebRtcFromGameplayToGameplayCommunicator
    (
        ICampaignServerCommunicator campaignServerCommunicator,
        IPacketHandler packetHandler,
        IPacketSerializer packetSerializer, 
        IChunkCollector chunkCollector
    )
    {
        _campaignServerCommunicator = campaignServerCommunicator;
        _campaignServerCommunicator.ConnectionEstablished += OnCampaignServerConnectionEstablished;
        
        _packetHandler = packetHandler;
        _packetSerializer = packetSerializer;
        
        _chunkCollector = chunkCollector;

        Name = nameof(WebRtcFromGameplayToGameplayCommunicator);
        ProcessPriority = (int)ProcessPriorityEnum.Communicator;
    }

    public override void _PhysicsProcess(float delta)
    {
        // Poll() is duplicated in _PhysicsProcess because it runs before _Process, so any packets received during
        // _Process would be delayed to the next physics frame otherwise.
        Poll();
        
        if (IsServer)
        {
            while (_packetQueue.Count > 0)
            {
                (Packet packet, Guid senderId) = _packetQueue.Dequeue();
                Try(senderId, () => _packetHandler.HandleAsync(packet, senderId));
            }
        }
    }

    public override void _Process(float delta)
    {
        Poll();
        ProcessPing(delta);
        
        _didPollThisFrame = false;
    }
    
    private void Poll()
    {
        if (_didPollThisFrame) return;
        _didPollThisFrame = true;
        
        foreach 
        ((
             Guid peerId, 
             (WebRTCPeerConnection connection, WebRTCDataChannel reliableChannel, WebRTCDataChannel unreliableChannel)
        ) in _peerConnectionsAndChannels.ToDictionary())
        {
            if (connection.GetConnectionState() == WebRTCPeerConnection.ConnectionState.Closed)
            {
                _peerConnectionsAndChannels.Remove(peerId);
                _ping.Remove(peerId);
                continue;
            }
            
            connection.Poll();
            HandlePackets(reliableChannel, peerId);
            HandlePackets(unreliableChannel, peerId);
        }
    }
    
    private void ProcessPing(double delta)
    {
        if (IsServer) return;
        _timeSinceLastPing += delta;
        if (_timeSinceLastPing >= PingInterval)
        {
            foreach ((Guid peerId, (Guid pingId, _)) in _ping.ToList())
                if (pingId != _lastPingId) _ping.Remove(peerId);
            _timeSinceLastPing = 0;
            _lastPingId = Guid.NewGuid();
            BroadcastUnreliable(new PingPacket { Id = _lastPingId });
        }
    }
    
    private void HandlePackets(WebRTCDataChannel channel, Guid senderId)
    {
        while (channel.GetAvailablePacketCount() > 0)
        {
            byte[] bytes = channel.GetPacket();
            BytesReceived += bytes.Length;
            HandlePacket(bytes, senderId);
        }
    }
    
    private void HandlePacket(byte[] bytes, Guid senderId)
    {
        Try(senderId, async () =>
        {
            Packet packet = _packetSerializer.Deserialize(bytes);
            if (packet is PingPacket pingPacket)
            {
                HandlePingPacket(pingPacket, senderId);
                return;
            }
            if (packet is ChunkPacket chunkPacket)
            {
                byte[]? restoredPacketBytes = _chunkCollector.AddChunk(chunkPacket, senderId);
                if (restoredPacketBytes != null)
                    HandlePacket(restoredPacketBytes, senderId);
                return;
            }
            
            // Server defers packet handling to _PhysicsProcess to ensure that all game logic is executed in it only
            if (IsServer)
                _packetQueue.Enqueue((packet, senderId));
            else
                await _packetHandler.HandleAsync(packet, senderId);
        });
    }
    
    private async void Try(Guid senderId, Func<Task> func)
    {
        try
        {
            await func();
        }
        catch (BadPacketException e)
        {
            if (IsServer)
                SendReliable(new BadInputPacket { Reason = e.Reason }, senderId);
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
    
    private void OnCampaignServerConnectionEstablished()
    {
        if (!IsServer)
            ConnectToShardServer(Const.TestShardId);
    }
    
    public void ConnectToShardServer(Guid peerId)
    {
        if (IsServer) throw new InvalidOperationException();
        WebRTCPeerConnection connection = CreateConnection(peerId);
        connection.CreateOffer();
    }
    
    private WebRTCPeerConnection CreateConnection(Guid peerId)
    {
        if (_peerConnectionsAndChannels.TryGetValue(peerId, out PeerConnectionAndChannels existing))
            existing.Connection.Close();

        var connection = new WebRTCPeerConnection();
        byte[] peerIdBytes = peerId.ToByteArray();
        connection.Connect("session_description_created", this, nameof(OnSessionDescriptionCreated), [peerIdBytes]);
        connection.Connect("ice_candidate_created", this, nameof(OnIceCandidateCreated), [peerIdBytes]);
        
        WebRTCDataChannel reliableChannel = connection.CreateDataChannel("reliable", new Dictionary
        {
            ["negotiated"] = true,
            ["id"] = 0,
        });
        WebRTCDataChannel unreliableChannel = connection.CreateDataChannel("unreliable", new Dictionary
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
        _campaignServerCommunicator.SendPacket(new WebrtcSdpPacket { Sdp = sdp, PeerId = peerId } );
    }
    
    private void OnIceCandidateCreated(string media, int index, String name, byte[] peerIdBytes)
    {
        var peerId = new Guid(peerIdBytes);
        _campaignServerCommunicator.SendPacket(new WebrtcIceCandidatePacket
        {
            Media = media,
            Index = index,
            Name = name,
            PeerId = peerId
        });
    }

    public void SendReliable(Packet packet, Guid receiverId) =>
        Send(_packetSerializer.Serialize(packet), receiverId, it => it.ReliableChannel);
    
    public void SendUnreliable(Packet packet, Guid receiverId) =>
        Send(_packetSerializer.Serialize(packet), receiverId, it => it.UnreliableChannel);
    
    public void BroadcastReliable(Packet packet) =>
        Broadcast(packet, it => it.ReliableChannel);
    
    public void BroadcastUnreliable(Packet packet) =>
        Broadcast(packet, it => it.UnreliableChannel);
    
    private void Broadcast(Packet packet, Func<PeerConnectionAndChannels, WebRTCDataChannel> channelSelector)
    {
        byte[] bytes = _packetSerializer.Serialize(packet);
        if (bytes.Length <= MaxChunkSize)
        {
            foreach (Guid receiverId in _peerConnectionsAndChannels.Keys)
                SendWithoutChunking(bytes, receiverId, channelSelector);
        }
        else
        {
            byte[][] chunks = SplitIntoChunks(bytes);
            foreach (Guid receiverId in _peerConnectionsAndChannels.Keys)
                foreach (byte[] chunk in chunks)
                    SendWithoutChunking(chunk, receiverId, channelSelector);
        }
    }
    
    private void Send
    (
        byte[] bytes,
        Guid receiverId,
        Func<PeerConnectionAndChannels, WebRTCDataChannel> channelSelector
    )
    {
        if (bytes.Length <= MaxChunkSize)
        {
            SendWithoutChunking(bytes, receiverId, channelSelector);
        }
        else
        {
            byte[][] chunks = SplitIntoChunks(bytes);
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
        string type = IsServer ? "offer" : "answer";
        WebRTCPeerConnection? connection = IsServer ? CreateConnection(packet.PeerId) :
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
}