using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Soteo.MasterServer.Extensions;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Enums;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Packets.Master;
using Soteo.Shared.Packets.Shared;

namespace Soteo.MasterServer.Controllers;

public sealed class WebSocketController : Controller
{
    private readonly IServiceProvider _requestServiceProvider;
    private readonly IPacketSender _packetSender;
    private readonly IWebSocketRepository _wsRepo;
    private readonly IUserRepository _userRepo;
    private readonly IDispatcher _dispatcher;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private Guid _userId;
    private ClaimsPrincipal? _claims;
    private DispatcherPriority _priority = DispatcherPriority.PlayerPacket;
    private readonly byte[] _receiveBuffer = new byte[1024];
    private WebSocket? _ws;
    private IPacketSerializer _packetSerializer;
    private ILogger<WebSocketController> _logger;

    public WebSocketController
    (
        IServiceProvider requestServiceProvider,
        IPacketSender packetSender,
        IWebSocketRepository wsRepo,
        IUserRepository userRepo,
        IDispatcher dispatcher,
        IConfiguration configuration,
        IPacketSerializer packetSerializer,
        ILogger<WebSocketController> logger
    )
    {
        _requestServiceProvider = requestServiceProvider;
        _packetSender = packetSender;
        _wsRepo = wsRepo;
        _userRepo = userRepo;
        _dispatcher = dispatcher;
        _packetSerializer = packetSerializer;
        _logger = logger;

        string intercomSecret = configuration["Soteo:IntercomSecret"] ??
                                throw new InvalidOperationException("Intercom secret is not set.");
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(intercomSecret))
        };
    }

    [Route("/")]
    public async Task Connect(CancellationToken ct)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        try
        {
            using WebSocket ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _ws = ws;
            _claims = await ReceiveAndValidateHandshakePacketAsync();
            if (_claims == null) return;
            _userId = _claims.Id;
            _priority = _claims.IsPlayer ? DispatcherPriority.PlayerPacket : DispatcherPriority.ServicePacket;

            try
            {
                await _wsRepo.CloseOldAndSetAsync(_userId, ws);
                await _dispatcher.InvokeSync(() => _userRepo.OnConnected(_claims), _priority);
                await ReceiveAndHandlePackets();
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, ct);
            }
            finally
            {
                await _dispatcher.InvokeSync(() => _userRepo.OnDisconnected(_userId), _priority);
                _wsRepo.Remove(_userId);
            }
        }
        catch (WebSocketException) {}
    }
    
    private async Task ReceiveAndHandlePackets()
    {
        WebSocketReceiveResult? receiveResult = null;
        while (receiveResult?.CloseStatus == null)
        {
            int size = 0;
            var segment = new ArraySegment<byte>(_receiveBuffer);
            do
            {
                receiveResult = await _ws!.ReceiveAsync(segment, CancellationToken.None);
                segment = segment[receiveResult.Count..];
                size += receiveResult.Count;
            } while (!receiveResult.EndOfMessage && segment.Count > 0);
            if (!receiveResult.EndOfMessage)
            {
                await _ws.CloseAsync(WebSocketCloseStatus.MessageTooBig, null, CancellationToken.None);
                return;
            }
            await HandlePacketAsync(new ArraySegment<byte>(_receiveBuffer, 0, size), receiveResult.MessageType);
        }
    }
    
    private async Task<ClaimsPrincipal?> ReceiveAndValidateHandshakePacketAsync()
    {
        WebSocketReceiveResult receiveResult = await _ws!.ReceiveAsync(_receiveBuffer, CancellationToken.None);
        
        HandshakePacket packet;
        try
        {
            packet = _packetSerializer.Deserialize(_receiveBuffer.AsSpan(..receiveResult.Count))
                as HandshakePacket ?? throw new BadPacketException("Handshake packet should be sent first");
        }
        catch (BadPacketException)
        {
            await _ws.CloseAsync
            (
                WebSocketCloseStatus.InvalidMessageType,
                "Invalid handshake packet format",
                CancellationToken.None
            );
            return null;
        }
        
        if (packet.Version != Const.Version)
        {
            await _ws.CloseAsync(WebSocketCloseStatus.ProtocolError, "Wrong client version", CancellationToken.None);
            return null;
        }
        
        TokenValidationResult? validationResult =
            await new JwtSecurityTokenHandler().ValidateTokenAsync(packet.Token, _tokenValidationParameters);
        if (!validationResult.IsValid)
        {
            await _ws.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid token", CancellationToken.None);
            return null;
        }
        
        return new ClaimsPrincipal(validationResult.ClaimsIdentity);
    }
    
    private async Task HandlePacketAsync(ArraySegment<byte> bytes, WebSocketMessageType type)
    {
        switch (type)
        {
            case WebSocketMessageType.Binary:
                await HandleBinaryPacketAsync(bytes);
                break;
            case WebSocketMessageType.Text:
                await _packetSender.SendToAsync(new BadInputPacket(), _userId);
                break;
            case WebSocketMessageType.Close:
                break;
        }
    }
    
    private async Task HandleBinaryPacketAsync(ArraySegment<byte> bytes)
    {
        Guid correlationId = Guid.Empty;
        Packet? packet = null;
        try
        {
            if (bytes.Count < 17) throw new BadPacketException("Packet is too short");
            var packetType = (PacketType)bytes[0];
            correlationId = new Guid(bytes[1..17]);
            packet = _packetSerializer.Deserialize(bytes);
            
            if (!TypeLocator.PacketHandlerTypes.TryGetValue(packetType, out Type? handlerType))
                throw new BadPacketException($"Packet type {packetType} can't be handled");
            await using AsyncServiceScope scope = _requestServiceProvider.CreateAsyncScope();
            var handler = (IPacketHandler)scope.ServiceProvider.GetRequiredService(handlerType);
            await _dispatcher.InvokeAsync
            (
                async () => await handler.HandleAsync(packet, _userRepo[_userId]),
                _priority
            );
        }
        catch (BadPacketException e)
        {
            await _packetSender
                .SendToAsync(new BadInputPacket { CorrelationId = correlationId, Reason = e.Reason }, _userId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unhandled exception while handling packet {packet}", packet);
            await _packetSender.SendToAsync(new InternalServerErrorPacket(), _userId);
        }
    }
}