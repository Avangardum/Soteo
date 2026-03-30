using Godot.Collections;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Shared;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Messages.Master;
using Soteo.Shared.Messages.PlayerShard;
using Soteo.Shared.Messages.Shared;
using Soteo.Shared.MessageSerializers;

namespace Soteo.Client;

public sealed class ClientShardServerCommunicator : Node, IMessageSender, IWebRtcSignalingReceiver
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
    private readonly IMessageSerializer _messageSerializer = new UniversalMessageSerializer();
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
            HandleMessages(reliableChannel, peerId);
            HandleMessages(unreliableChannel, peerId);
        }
    }
    
    private void HandleMessages(WebRTCDataChannel channel, Guid senderId)
    {
        while (channel.GetAvailablePacketCount() > 0)
        {
            byte[] bytes = channel.GetPacket();
            try
            {
                Message message = _messageSerializer.Deserialize(bytes);
                if (!TypeLocator.MessageHandlerTypes.TryGetValue(message.Type, out Type? handlerType))
                    throw new BadMessageException($"Can't handle message of type {message.Type}");
                var handler = (IMessageHandler)_serviceProvider.GetRequiredService(handlerType);
                handler.HandleAsync(message, senderId);
            }
            catch (BadMessageException e)
            {
                if (IsServer) SendReliable(new BadInputMessage { Reason = e.Reason }, senderId);
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
        _masterServerCommunicator.SendMessage(new WebrtcSdpMessage { Sdp = sdp, PeerId = peerId } );
    }
    
    private void OnIceCandidateCreated(string media, int index, String name, byte[] peerIdBytes)
    {
        var peerId = new Guid(peerIdBytes);
        _masterServerCommunicator.SendMessage(new WebrtcIceCandidateMessage
        {
            Media = media,
            Index = index,
            Name = name,
            PeerId = peerId
        });
    }

    public void SendReliable(Message message, Guid receiverId)
    {
        if (!_peerConnectionsAndChannels.TryGetValue(receiverId, out var connectionAndChannels)) return;
        byte[] bytes = _messageSerializer.Serialize(message);
        connectionAndChannels.ReliableChannel.PutPacket(bytes);
    }
    
    public void SendUnreliable(Message message, Guid receiverId)
    {
        if (!_peerConnectionsAndChannels.TryGetValue(receiverId, out var connectionAndChannels)) return;
        byte[] bytes = _messageSerializer.Serialize(message);
        connectionAndChannels.UnreliableChannel.PutPacket(bytes);
    }
    
    public void SetRemoteDescription(WebrtcSdpMessage message)
    {
        string type = IsServer ? "offer" : "answer";
        WebRTCPeerConnection? connection = IsServer ? CreateConnection(message.PeerId) :
            _peerConnectionsAndChannels.GetOrDefault(message.PeerId)?.Connection;
        if (connection == null) return;
        connection.SetRemoteDescription(type, message.Sdp);
        GD.Print("Set remote description");
        GD.Print(message.Sdp);
        if (!IsServer) Task.Delay(1000).ContinueWithinContext(_ =>
        {
            GD.Print("Sending move");
            SendReliable(new MoveMessage(), message.PeerId);
            SendUnreliable(new MoveMessage(), message.PeerId);
        }); // todo remove
    }
    
    public void AddRemoteIceCandidate(WebrtcIceCandidateMessage message)
    {
        WebRTCPeerConnection? connection = _peerConnectionsAndChannels.GetOrDefault(message.PeerId)?.Connection;
        if (connection == null) return;
        connection.AddIceCandidate(message.Media, message.Index, message.Name);
        GD.Print("Added remote ice candidate");
        GD.Print(message.Media);
        GD.Print(message.Index);
        GD.Print(message.Name);
    }
}