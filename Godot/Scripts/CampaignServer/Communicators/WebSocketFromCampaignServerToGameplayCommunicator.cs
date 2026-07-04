using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using Soteo.Core;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Exceptions;
using Soteo.Core.Interfaces;
using Soteo.Core.Models;
using Soteo.Core.StaticHelpers;
using Soteo.Util;

namespace Soteo.Main.CampaignServer.Communicators;

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
    
    public void SendTo(Packet packet, params IEnumerable<Guid> receiverIds)
    {
        byte[] bytes = _packetSerializer.Serialize(packet);
        foreach (Guid id in receiverIds)
            if (_userIdsByWsPeerId.Inverse.TryGetValue(id, out int wsPeerId))
                _wsServer.GetPeer(wsPeerId).PutPacket(bytes).ThrowIfError();
    }
    
    public void BroadcastToAll(Packet packet)
    {
        byte[] bytes = _packetSerializer.Serialize(packet);
        foreach (int wsPeerId in _userIdsByWsPeerId.Keys)
            _wsServer.GetPeer(wsPeerId).PutPacket(bytes).ThrowIfError();
    }
    
    public void BroadcastToShardServers(Packet packet) =>
        BroadcastToUsersWhere(packet, it => it.IsShard);
    
    public void BroadcastToClients(Packet packet) =>
        BroadcastToUsersWhere(packet, it => it.IsPlayer);
    
    private void BroadcastToUsersWhere(Packet packet, Func<User, bool> predicate)
    {
        byte[] bytes = _packetSerializer.Serialize(packet);
        IEnumerable<int> wsPeerIds = _userIdsByWsPeerId
            .Where(it => _userRepo.TryGetValue(it.Value, out User user) && predicate(user))
            .Select(it => it.Key);
        foreach (int wsPeerId in wsPeerIds)
            _wsServer.GetPeer(wsPeerId).PutPacket(bytes).ThrowIfError();
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
        catch (BadSerializedDataException e)
        {
            peer.PutPacket(_packetSerializer.Serialize(new BadInputPacket { Reason = e.Message } )).ThrowIfError();
            return null;
        }
    }
    
    private void HandleHandshakePacket(Packet packet, int wsPeerId, WebSocketPeer peer)
    {
        if (packet is not CampaignServerHandshakePacket handshake)
        {
            peer.PutPacket(_packetSerializer.Serialize(new BadInputPacket { Reason = "Handshake expected" } ))
                .ThrowIfError();
            return;
        }
        if (handshake.Version != Const.Version)
        {
            peer.PutPacket(_packetSerializer.Serialize(new BadInputPacket { Reason = "Version mismatch" } ))
                .ThrowIfError();
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
            peer.PutPacket(_packetSerializer.Serialize(new BadInputPacket { Reason = "Invalid token" } ))
                .ThrowIfError();
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
            SendTo(new BadInputPacket { Reason = e.Message }, senderId);
        }
        catch (Exception e)
        {
            AsyncExceptionCollector.Collect(e);
        }
    }
}
