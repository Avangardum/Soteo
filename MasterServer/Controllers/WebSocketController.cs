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
using Soteo.Shared.Messages.Master;
using Soteo.Shared.MessageSerializers;

namespace Soteo.MasterServer.Controllers;

public sealed class WebSocketController : Controller
{
    private readonly HandshakeMessageSerializer _handshakeMessageSerializer =
        (HandshakeMessageSerializer)TypeLocator.MessageSerializers[MessageType.Handshake];

    private readonly IServiceProvider _requestServiceProvider;
    private readonly IMessageSender _messageSender;
    private readonly IWebSocketRepository _wsRepo;
    private readonly IUserRepository _userRepo;
    private readonly IDispatcher _dispatcher;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private Guid _userId;
    private ClaimsPrincipal? _claims;
    private DispatcherPriority _priority = DispatcherPriority.PlayerMessage;
    private readonly byte[] _receiveBuffer = new byte[1024];
    private WebSocket? _ws;

    public WebSocketController
    (
        IServiceProvider requestServiceProvider,
        IMessageSender messageSender,
        IWebSocketRepository wsRepo,
        IUserRepository userRepo,
        IDispatcher dispatcher,
        IConfiguration configuration
    )
    {
        _requestServiceProvider = requestServiceProvider;
        _messageSender = messageSender;
        _wsRepo = wsRepo;
        _userRepo = userRepo;
        _dispatcher = dispatcher;
        
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
            _claims = await ReceiveAndValidateHandshakeMessageAsync();
            if (_claims == null) return;
            _userId = _claims.Id;
            _priority = _claims.IsPlayer ? DispatcherPriority.PlayerMessage : DispatcherPriority.ServiceMessage;

            try
            {
                await _wsRepo.CloseOldAndSetAsync(_userId, ws);
                await _dispatcher.InvokeSync(() => _userRepo.OnConnected(_claims), _priority);
                await ReceiveAndHandleMessages();
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
    
    private async Task ReceiveAndHandleMessages()
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
            await HandleMessageAsync(new ArraySegment<byte>(_receiveBuffer, 0, size), receiveResult.MessageType);
        }
    }
    
    private async Task<ClaimsPrincipal?> ReceiveAndValidateHandshakeMessageAsync()
    {
        WebSocketReceiveResult receiveResult = await _ws!.ReceiveAsync(_receiveBuffer, CancellationToken.None);
        
        HandshakeMessage message;
        try
        {
            message = _handshakeMessageSerializer.Deserialize(_receiveBuffer.AsSpan(..receiveResult.Count));
        }
        catch (InvalidMessageException)
        {
            await _ws.CloseAsync
            (
                WebSocketCloseStatus.InvalidMessageType,
                "Invalid handshake message format.",
                CancellationToken.None
            );
            return null;
        }
        
        if (message.Version != Const.Version)
        {
            await _ws.CloseAsync(WebSocketCloseStatus.ProtocolError, "Wrong client version.", CancellationToken.None);
            return null;
        }
        
        TokenValidationResult? validationResult =
            await new JwtSecurityTokenHandler().ValidateTokenAsync(message.Token, _tokenValidationParameters);
        if (!validationResult.IsValid)
        {
            await _ws.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid token.", CancellationToken.None);
            return null;
        }
        
        return new ClaimsPrincipal(validationResult.ClaimsIdentity);
    }
    
    private async Task HandleMessageAsync(ArraySegment<byte> bytes, WebSocketMessageType type)
    {
        switch (type)
        {
            case WebSocketMessageType.Binary:
                await HandleBinaryMessageAsync(bytes);
                break;
            case WebSocketMessageType.Text:
                await _messageSender.SendToAsync(new InvalidMessageMessage(), _userId);
                break;
            case WebSocketMessageType.Close:
                break;
        }
    }
    
    private async Task HandleBinaryMessageAsync(ArraySegment<byte> bytes)
    {
        Guid correlationId = Guid.Empty;
        try
        {
            if (bytes.Count < 17) throw new InvalidMessageException();
            var messageType = (MessageType)bytes[0];
            correlationId = new Guid(bytes[1..17]);
            if (!TypeLocator.MessageSerializers.TryGetValue(messageType, out IMessageSerializer? serializer))
                throw new InvalidMessageException();
            var message = serializer.Deserialize(bytes);
            
            if (!TypeLocator.MessageHandlerTypes.TryGetValue(messageType, out Type? handlerType))
                throw new InvalidMessageException();
            await using AsyncServiceScope scope = _requestServiceProvider.CreateAsyncScope();
            var handler = (IMessageHandler)scope.ServiceProvider.GetRequiredService(handlerType);
            await _dispatcher.InvokeAsync
            (
                async () => await handler.HandleAsync(message, _userRepo[_userId]),
                _priority
            );
        }
        catch (InvalidMessageException)
        {
            await _messageSender.SendToAsync(new InvalidMessageMessage { CorrelationId = correlationId }, _userId);
        }
    }
}