using Godot.Collections;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Attributes;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Nodes.Autoloads;
using Soteo.Shared.Packets;
using Soteo.Shared.PacketSerializers;

namespace Soteo.Gameplay.Nodes.Systems.Communicators;

/// <summary>
/// Communicates between clients and shard servers
/// </summary>
public sealed class WebRtcGameplayCommunicator : Node, IPacketSender, IWebrtcPacketReceiver, IPingMeasurer
{
    private record PeerConnectionAndChannels
    (
        WebRTCPeerConnection Connection,
        WebRTCDataChannel ReliableChannel,
        WebRTCDataChannel UnreliableChannel
    );
    
    
    private const float PingInterval = 1;
    private float _timeSinceLastPing;
    private Guid _lastPingId;
    private readonly System.Collections.Generic.Dictionary<Guid, PeerConnectionAndChannels>
        _peerConnectionsAndChannels = [];
    private readonly System.Collections.Generic.Dictionary<Guid, (Guid PingId, float ResponseTime)> _ping = [];
    
    private IMasterServerCommunicator _masterServerCommunicator = null!;
    private readonly IPacketSerializer _packetSerializer = new RoutingPacketSerializer();
    private IPacketHandler _packetHandler = null!;

    [Inject]
    public void Inject(IMasterServerCommunicator masterServerCommunicator, IPacketHandler packetHandler)
    {
        _masterServerCommunicator = masterServerCommunicator;
        _masterServerCommunicator.ConnectionEstablished += OnMasterServerConnectionEstablished;
        
        _packetHandler = packetHandler;
    }

    public override void _Ready()
    {
        if (UseJsmq)
        {
            SetProcess(false);
            SetPhysicsProcess(false);
            QueueFree();
            return;
        }
        
        ProcessPriority = (int)ProcessPriorityEnum.Communicator;
    }

    public override void _PhysicsProcess(float delta)
    {
        // Server polls in _PhysicsProcess so that simulation code only runs on physics ticks
        if (IsServer) Poll(delta);
    }

    public override void _Process(float delta)
    {
        // Client polls in _Process to minimize latency
        if (!IsServer) Poll(delta);
    }

    private void Poll(float delta)
    {
        foreach 
        ((
             Guid peerId, 
             (WebRTCPeerConnection connection, WebRTCDataChannel reliableChannel, WebRTCDataChannel unreliableChannel)
        ) in _peerConnectionsAndChannels)
        {
            connection.Poll();
            HandlePackets(reliableChannel, peerId);
            HandlePackets(unreliableChannel, peerId);
        }
        
        if (!IsServer)
        {
            _timeSinceLastPing += delta;
            if (_timeSinceLastPing >= PingInterval)
            {
                foreach ((Guid peerId, (Guid pingId, _)) in _ping)
                    if (pingId != _lastPingId) _ping.Remove(peerId);
                _timeSinceLastPing = 0;
                _lastPingId = Guid.NewGuid();
                BroadcastUnreliable(new PingPacket { Id = _lastPingId });
            }
        }
    }
    
    private void HandlePackets(WebRTCDataChannel channel, Guid senderId)
    {
        while (channel.GetAvailablePacketCount() > 0)
        {
            byte[] bytes = channel.GetPacket();
            HandlePacket(bytes, senderId);
        }
    }
    
    private async void HandlePacket(byte[] bytes, Guid senderId)
    {
        try
        {
            Packet packet = _packetSerializer.Deserialize(bytes);
            if (packet is PingPacket pingPacket)
            {
                HandlePingPacket(pingPacket, senderId);
                return;
            }
            await _packetHandler.HandleAsync(packet, senderId);
        }
        catch (BadPacketException e)
        {
            if (IsServer) SendReliable(new BadInputPacket { Reason = e.Reason }, senderId);
            else AsyncExceptionCollector.Collect(e);
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
    
    private void OnMasterServerConnectionEstablished()
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
        // todo support reconnection
        if (_peerConnectionsAndChannels.ContainsKey(peerId)) throw new InvalidOperationException("Already created");
        
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
            ["maxRetransmits"] = 0
        });
        
        _peerConnectionsAndChannels[peerId] =
            new PeerConnectionAndChannels(connection, reliableChannel, unreliableChannel);
        return connection;
    }
    
    private void OnSessionDescriptionCreated(string type, string sdp, byte[] peerIdBytes)
    {
        var peerId = new Guid(peerIdBytes);
        _peerConnectionsAndChannels[peerId].Connection.SetLocalDescription(type, sdp);
        _masterServerCommunicator.SendPacket(new WebrtcSdpPacket { Sdp = sdp, PeerId = peerId } );
    }
    
    private void OnIceCandidateCreated(string media, int index, String name, byte[] peerIdBytes)
    {
        var peerId = new Guid(peerIdBytes);
        _masterServerCommunicator.SendPacket(new WebrtcIceCandidatePacket
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
    
    public void BroadcastReliable(Packet packet)
    {
        byte[] bytes = _packetSerializer.Serialize(packet);
        foreach (var receiverId in _peerConnectionsAndChannels.Keys)
            Send(bytes, receiverId, it => it.ReliableChannel);
    }
    
    public void BroadcastUnreliable(Packet packet)
    {
        byte[] bytes = _packetSerializer.Serialize(packet);
        foreach (var receiverId in _peerConnectionsAndChannels.Keys)
            Send(bytes, receiverId, it => it.UnreliableChannel);
    }
    
    private void Send
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
    }
    
    public void ReceiveWebrtcSdpPacket(WebrtcSdpPacket packet)
    {
        string type = IsServer ? "offer" : "answer";
        WebRTCPeerConnection? connection = IsServer ? CreateConnection(packet.PeerId) :
            _peerConnectionsAndChannels.GetOrDefault(packet.PeerId)?.Connection;
        if (connection == null) return;
        connection.SetRemoteDescription(type, packet.Sdp);
    }
    
    public void ReceiveWebrtcIceCandidatePacket(WebrtcIceCandidatePacket packet)
    {
        WebRTCPeerConnection? connection = _peerConnectionsAndChannels.GetOrDefault(packet.PeerId)?.Connection;
        if (connection == null) return;
        connection.AddIceCandidate(packet.Media, packet.Index, packet.Name);
    }

    public float? Ping(Guid peerId) =>
        _ping.TryGetValue(peerId, out var tuple) ? tuple.ResponseTime : null;
}