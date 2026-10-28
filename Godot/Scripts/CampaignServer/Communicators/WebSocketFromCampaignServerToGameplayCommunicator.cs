using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using Soteo.Core.Dto.Options;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Exceptions;
using Soteo.Core.Interfaces;
using Soteo.Core.Models;
using Soteo.Core.StaticHelpers;
using Soteo.Util;

namespace Soteo.Main.CampaignServer.Communicators;

public sealed partial class WebSocketFromCampaignServerToGameplayCommunicator :
    GdObject, IFromCampaignServerCommunicator
{
    private readonly TcpServer _tcpServer = new();
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketHandler _packetHandler;
    private readonly IUserRepository _userRepo;
    private readonly JwtBuilder _jwtBuilder;
    private readonly IInitializationRepository _initRepo;
    private readonly CertificateOptions _certificateOptions;

    private readonly BidirectionalDictionary<long, Guid> _userIdsByWsPeerId = [];
    private readonly Dictionary<long, WebSocketPeer> _wsPeers = [];
    private long _nextWsPeerId;
    private readonly Dictionary<WebSocketPeer, Task> _inProgressHandshakeTasksByWsPeer = [];

    public event Action<Guid> PeerConnected = delegate { };
    public event Action<Guid> PeerDisconnected = delegate { };

    public WebSocketFromCampaignServerToGameplayCommunicator
    (
        IPacketSerializer packetSerializer,
        IPacketHandler packetHandler,
        IUserRepository userRepo,
        IInitializationRepository initRepo,
        IntercomOptions intercomOptions,
        CertificateOptions certificateOptions,
        PortOptions portOptions
    )
    {
        _packetSerializer = packetSerializer;
        _packetHandler = packetHandler;
        _userRepo = userRepo;
        _initRepo = initRepo;
        _certificateOptions = certificateOptions;

        _jwtBuilder = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(Convert.FromBase64String(intercomOptions.IntercomSecret));

        _tcpServer.Listen((ushort)portOptions.Port).ThrowIfError();
    }

    public void SendTo(Packet packet, params IEnumerable<Guid> receiverIds)
    {
        byte[] bytes = _packetSerializer.Serialize(packet);
        foreach (Guid id in receiverIds)
            if (_userIdsByWsPeerId.Inverse.TryGetValue(id, out long wsPeerId))
                _wsPeers[wsPeerId].PutPacket(bytes).ThrowIfError();
    }

    public void BroadcastToAll(Packet packet)
    {
        byte[] bytes = _packetSerializer.Serialize(packet);
        foreach (long wsPeerId in _userIdsByWsPeerId.Keys)
            _wsPeers[wsPeerId].PutPacket(bytes).ThrowIfError();
    }

    public void BroadcastToShardServers(Packet packet) =>
        BroadcastToUsersWhere(packet, it => it.IsShard);

    public void BroadcastToClients(Packet packet) =>
        BroadcastToUsersWhere(packet, it => it.IsPlayer);

    private void BroadcastToUsersWhere(Packet packet, Func<User, bool> predicate)
    {
        byte[] bytes = _packetSerializer.Serialize(packet);
        IEnumerable<long> wsPeerIds = _userIdsByWsPeerId
            .Where(it => _userRepo.TryGetValue(it.Value, out User? user) && predicate(user))
            .Select(it => it.Key);
        foreach (long wsPeerId in wsPeerIds)
            _wsPeers[wsPeerId].PutPacket(bytes).ThrowIfError();
    }

    public void RelayFrom(RelayedPacket packet, Guid senderId) =>
        SendTo(packet with { PeerId = senderId }, packet.PeerId);

    public void Poll()
    {
        AcceptNewConnections();
        foreach ((long wsPeerId, WebSocketPeer wsPeer) in _wsPeers)
            PollWsPeer(wsPeerId, wsPeer);
    }

    private void AcceptNewConnections()
    {
        while (_tcpServer.IsConnectionAvailable())
        {
            StreamPeerTcp rawTcpStream = _tcpServer.TakeConnection();
            var tlsTcpStream = new StreamPeerTls();
            var certificate = new X509Certificate();
            certificate.Load(_certificateOptions.CertificatePath);
            var key = new CryptoKey();
            key.Load(_certificateOptions.PrivateKeyPath);
            var tlsOptions = TlsOptions.Server(key, certificate);
            tlsTcpStream.AcceptStream(rawTcpStream, tlsOptions).ThrowIfError();
            var wsPeer = new WebSocketPeer();
            wsPeer.AcceptStream(tlsTcpStream);
            _wsPeers[_nextWsPeerId++] = wsPeer;
        }
    }

    private void PollWsPeer(long wsPeerId, WebSocketPeer wsPeer)
    {
        wsPeer.Poll();
        switch (wsPeer.GetReadyState())
        {
            case WebSocketPeer.State.Open:
                while (wsPeer.GetAvailablePacketCount() > 0)
                {
                    Packet? packet = GetPacket(wsPeer);
                    if (packet == null) return;
                    HandlePacketAsync(packet, wsPeerId, wsPeer).CollectException();
                }
                break;
            case WebSocketPeer.State.Closed:
                if (_userIdsByWsPeerId.TryGetValue(wsPeerId, out Guid userId))
                {
                    _userRepo.OnDisconnected(userId);
                    PeerDisconnected(userId);
                }
                _wsPeers.Remove(wsPeerId);
                _userIdsByWsPeerId.Remove(wsPeerId);
                break;
        }
    }

    private async Task HandlePacketAsync(Packet packet, long wsPeerId, WebSocketPeer wsPeer)
    {
        if (!_userIdsByWsPeerId.TryGetValue(wsPeerId, out Guid userId))
        {
            if (_inProgressHandshakeTasksByWsPeer.TryGetValue(wsPeer, out Task? handshakeTask))
            {
                await handshakeTask;
            }
            else
            {
                handshakeTask = HandleHandshakePacketAsync(packet, wsPeerId, wsPeer);
                if (!handshakeTask.IsCompleted)
                {
                    _inProgressHandshakeTasksByWsPeer[wsPeer] = handshakeTask;
                    handshakeTask.ContinueWithinContext(() => _inProgressHandshakeTasksByWsPeer.Remove(wsPeer))
                        .CollectException();
                }
                return;
            }
        }

        HandlePacket(packet, userId);
    }

    private Packet? GetPacket(WebSocketPeer peer)
    {
        byte[] bytes = peer.GetPacket();
        try
        {
            return _packetSerializer.Deserialize(bytes);
        }
        catch (Exception e)
        {
            peer.PutPacket(_packetSerializer.Serialize(new BadInputPacket { Reason = e.Message } )).ThrowIfError();
            return null;
        }
    }

    private async Task HandleHandshakePacketAsync(Packet packet, long wsPeerId, WebSocketPeer wsPeer)
    {
        void Fail(string reason)
        {
            wsPeer.PutPacket(_packetSerializer.Serialize(new BadInputPacket { Reason = reason })).ThrowIfError();
            wsPeer.Close(reason: "Bad handshake");
        }

        if (packet is not CampaignServerHandshakePacket handshake)
        {
            Fail("Handshake expected");
            return;
        }
        if (handshake.Version != Const.Version)
        {
            Fail("Version mismatch");
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
            Fail("Invalid token");
            return;
        }

        Guid userId = Guid.Parse((string)claims["sub"]);
        bool isPlayer =
            claims.TryGetValue("player", out object? value) &&
            value is string strValue &&
            bool.TryParse(strValue, out bool parsedValue) &&
            parsedValue;

        if (isPlayer && !_initRepo.IsInitialized)
            await _initRepo.WaitForInitAsync();

        if (_userIdsByWsPeerId.Inverse.TryGetValue(userId, out long oldWsPeerId))
        {
            _wsPeers[oldWsPeerId].Close(reason: "New connection opened");
            _userIdsByWsPeerId.Remove(oldWsPeerId);
            PeerDisconnected(userId);
        }

        _userIdsByWsPeerId[wsPeerId] = userId;
        _userRepo.OnConnected(claims);
        PeerConnected(userId);
        SendTo(new CampaignServerHandshakeAckPacket(), userId);
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
