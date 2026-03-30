using System.Threading.Tasks;
using Godot.Collections;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Client.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Packets.MasterServer;
using Soteo.Shared.Packets.PlayerShardServer;
using Soteo.Shared.Packets.Shared;
using Soteo.Shared.PacketSerializers;
using Soteo.Shared.PacketSerializers.Shared;

namespace Soteo.Client.Nodes.Autoloads;

public sealed class ClientShardServerCommunicator : Node, IPacketSender, IWebRtcSignalingReceiver
{
    private record PeerConnectionAndChannels
    (
        WebRTCPeerConnection Connection,
        WebRTCDataChannel ReliableChannel,
        WebRTCDataChannel UnreliableChannel
    );
    
    private readonly System.Collections.Generic.Dictionary<Guid, PeerConnectionAndChannels>
        _peerConnectionsAndChannels = [];
    private IMasterServerCommunicator _masterServerCommunicator = null!;
    private readonly IPacketSerializer _packetSerializer = new UniversalPacketSerializer();
    private IServiceProvider _serviceProvider = null!;

    public void Inject(IMasterServerCommunicator masterServerCommunicator, IServiceProvider serviceProvider)
    {
        _masterServerCommunicator = masterServerCommunicator;
        _masterServerCommunicator.ConnectionEstablished += OnMasterServerConnectionEstablished;
        
        _serviceProvider = serviceProvider;
    }
    
    public override void _PhysicsProcess(float delta)
    {
        // Server polls in _PhysicsProcess so that the simulation code only runs on physics ticks
        if (IsServer) Poll();
    }

    public override void _Process(float delta)
    {
        // Client polls in _Process to minimize latency
        if (!IsServer) Poll();
    }

    private void Poll()
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
    }
    
    private void HandlePackets(WebRTCDataChannel channel, Guid senderId)
    {
        while (channel.GetAvailablePacketCount() > 0)
        {
            byte[] bytes = channel.GetPacket();
            try
            {
                Packet packet = _packetSerializer.Deserialize(bytes);
                if (!TypeLocator.PacketHandlerTypes.TryGetValue(packet.Type, out Type? handlerType))
                    throw new BadPacketException($"Can't handle packet of type {packet.Type}");
                var handler = (IPacketHandler)_serviceProvider.GetRequiredService(handlerType);
                handler.HandleAsync(packet, senderId);
            }
            catch (BadPacketException e)
            {
                if (IsServer) SendReliable(new BadInputPacket { Reason = e.Reason }, senderId);
                else throw;
            }
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

    public void SendReliable(Packet packet, Guid receiverId)
    {
        if (!_peerConnectionsAndChannels.TryGetValue(receiverId, out var connectionAndChannels)) return;
        byte[] bytes = _packetSerializer.Serialize(packet);
        connectionAndChannels.ReliableChannel.PutPacket(bytes);
    }
    
    public void SendUnreliable(Packet packet, Guid receiverId)
    {
        if (!_peerConnectionsAndChannels.TryGetValue(receiverId, out var connectionAndChannels)) return;
        byte[] bytes = _packetSerializer.Serialize(packet);
        connectionAndChannels.UnreliableChannel.PutPacket(bytes);
    }
    
    public void SetRemoteDescription(WebrtcSdpPacket packet)
    {
        string type = IsServer ? "offer" : "answer";
        WebRTCPeerConnection? connection = IsServer ? CreateConnection(packet.PeerId) :
            _peerConnectionsAndChannels.GetOrDefault(packet.PeerId)?.Connection;
        if (connection == null) return;
        connection.SetRemoteDescription(type, packet.Sdp);
        GD.Print("Set remote description");
        GD.Print(packet.Sdp);
        if (!IsServer) Task.Delay(1000).ContinueWithinContext(_ =>
        {
            GD.Print("Sending move");
            SendReliable(new MovePacket(), packet.PeerId);
            SendUnreliable(new MovePacket(), packet.PeerId);
        }); // todo remove
    }
    
    public void AddRemoteIceCandidate(WebrtcIceCandidatePacket packet)
    {
        WebRTCPeerConnection? connection = _peerConnectionsAndChannels.GetOrDefault(packet.PeerId)?.Connection;
        if (connection == null) return;
        connection.AddIceCandidate(packet.Media, packet.Index, packet.Name);
        GD.Print("Added remote ice candidate");
        GD.Print(packet.Media);
        GD.Print(packet.Index);
        GD.Print(packet.Name);
    }
}