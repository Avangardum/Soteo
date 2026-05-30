using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Exceptions;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Nodes.Autoloads;
using Soteo.Util;

namespace Soteo.CampaignServer.Communicators;

public sealed class WebSocketFromCampaignServerToGameplayCommunicator : GdObject, ICommunicator
{
    private readonly WebSocketServer _wsServer = new();
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketHandler _packetHandler;
    private readonly IUserRepository _userRepo;
    private readonly JwtBuilder _jwtBuilder;
    
    private readonly BidirectionalDictionary<int, Guid> _userIdsByWsPeerId = [];
    
    public WebSocketFromCampaignServerToGameplayCommunicator
    (
        IPacketSerializer packetSerializer,
        IPacketHandler packetHandler,
        IUserRepository userRepo
    )
    {
        _packetSerializer = packetSerializer;
        _packetHandler = packetHandler;
        _userRepo = userRepo;
        
        string intercomSecret = SysEnvironment.GetEnvironmentVariable("Soteo__IntercomSecret") ??
            throw new Exception("Intercom secret is not set");
        _jwtBuilder = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(Convert.FromBase64String(intercomSecret));

        _wsServer.SslCertificate = new X509Certificate();
        _wsServer.SslCertificate.Load("res://devcert.crt");
        _wsServer.PrivateKey = new CryptoKey();
        string privateKeyPath = SysEnvironment.GetEnvironmentVariable("Soteo__PrivateKeyPath") ??
            throw new Exception("Private key path is not set");
        _wsServer.PrivateKey.Load(privateKeyPath);
        _wsServer.Listen(3706);
        _wsServer.Connect("client_disconnected", this, nameof(OnClientDisconnected));
        _wsServer.Connect("data_received", this, nameof(OnDataReceived));
    }
    
    public void SendTo(Packet packet, Guid receiverId)
    {
        if (_userIdsByWsPeerId.Inverse.TryGetValue(receiverId, out int wsPeerId))
        {
            byte[] bytes = _packetSerializer.Serialize(packet);
            _wsServer.GetPeer(wsPeerId).PutPacket(bytes);
        }
    }

    public void RelayFrom(RelayedPacket packet, Guid senderId) =>
        SendTo(packet with { PeerId = senderId }, packet.PeerId);

    public void Poll()
    {
        _wsServer.Poll();
    }
    
    private void OnClientDisconnected(int wsPeerId, bool wasClean)
    {
        if (_userIdsByWsPeerId.TryGetValue(wsPeerId, out Guid userId))
            _userRepo.OnDisconnected(userId);
        _userIdsByWsPeerId.Remove(wsPeerId);
    }
    
    private void OnDataReceived(int wsPeerId)
    {
        WebSocketPeer wsPeer = _wsServer.GetPeer(wsPeerId);
        Packet? packet = GetPacket(wsPeer);
        if (packet == null) return;
        
        if (!_userIdsByWsPeerId.TryGetValue(wsPeerId, out Guid userId))
        {
            HandleHandshakePacket(packet, wsPeerId, wsPeer);
        }
        else
        {
            HandlePacket(packet, userId);
        }
    }
    
    private Packet? GetPacket(WebSocketPeer peer)
    {
        byte[] bytes = peer.GetPacket();
        try
        {
            return _packetSerializer.Deserialize(bytes);
        }
        catch (BadPacketException e)
        {
            peer.PutPacket(_packetSerializer.Serialize(new BadInputPacket { Reason = e.Reason } ));
            return null;
        }
    }
    
    private void HandleHandshakePacket(Packet packet, int wsPeerId, WebSocketPeer peer)
    {
        if (packet is not CampaignServerHandshakePacket handshake)
        {
            peer.PutPacket(_packetSerializer.Serialize(new BadInputPacket { Reason = "Handshake expected" } ));
            return;
        }
        if (handshake.Version != Const.Version)
        {
            peer.PutPacket(_packetSerializer.Serialize(new BadInputPacket { Reason = "Version mismatch" } ));
            return;
        }
        
        Dictionary<string, object> claims;
        try
        {
            claims = _jwtBuilder.Decode<Dictionary<string, object>>(handshake.Token);
        }
        catch (Exception e)
        {
            if (e is not (TokenNotYetValidException or TokenExpiredException or SignatureVerificationException))
                throw;
            peer.PutPacket(_packetSerializer.Serialize(new BadInputPacket { Reason = "Invalid token" } ));
            return;
        }
        var userId = Guid.Parse((string)claims["sub"]);
     
        if (_userIdsByWsPeerId.Inverse.TryGetValue(userId, out int oldWsPeerId))
        {
            _wsServer.DisconnectPeer(oldWsPeerId, 1000, "New connection opened");
            _userIdsByWsPeerId.Remove(oldWsPeerId);
        }
        
        _userRepo.OnConnected(claims);
        _userIdsByWsPeerId[wsPeerId] = userId;
    }
    
    private async void HandlePacket(Packet packet, Guid senderId)
    {
        try
        {
            await _packetHandler.HandleAsync(packet, senderId);
        }
        catch (BadPacketException e)
        {
            SendTo(new BadInputPacket { Reason = e.Reason}, senderId);
        }
        catch (Exception e)
        {
            AsyncExceptionCollector.Collect(e);
        }
    }
}
