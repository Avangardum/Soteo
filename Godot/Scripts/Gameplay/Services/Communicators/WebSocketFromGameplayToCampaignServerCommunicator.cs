using System.Text;
using JWT.Builder;
using Soteo.Core.Dto.Options;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.SidedDependencies;
using Soteo.Core.StaticHelpers;

namespace Soteo.Main.Gameplay.Services.Communicators;

public sealed class WebSocketFromGameplayToCampaignServerCommunicator :
    Node, IFromGameplayToCampaignServerPacketSender, ICampaignServerConnector
{
    private readonly WebSocketClient _wsClient = new();
    private readonly HTTPRequest _httpRequest = new() { Name = "AuthHttpRequest", Timeout = 15 };

    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketHandler _packetHandler;
    private readonly ICurrentUserIdRepository _currentUserIdRepository;
    private readonly SideOptions _sideOptions;

    private readonly ToAuthServerConnectionOptions _toAuthServerConnectionOptions;
    private readonly ToCampaignServerConnectionOptions _toCampaignServerConnectionOptions;
    private readonly CertificateVerificationOptions _certificateVerificationOptions;
    private readonly ServerDependency<IntercomOptions> _intercomOptions;

    private string? _token;
    private Status _status;

    public WebSocketFromGameplayToCampaignServerCommunicator
    (
        IPacketHandler packetHandler,
        IPacketSerializer packetSerializer,
        ICurrentUserIdRepository currentUserIdRepository,
        SideOptions sideOptions,
        ToAuthServerConnectionOptions toAuthServerConnectionOptions,
        ToCampaignServerConnectionOptions toCampaignServerConnectionOptions,
        CertificateVerificationOptions certificateVerificationOptions,
        ServerDependency<IntercomOptions> intercomOptions
    )
    {
        _packetHandler = packetHandler;
        _packetSerializer = packetSerializer;
        _sideOptions = sideOptions;
        _currentUserIdRepository = currentUserIdRepository;
        _toAuthServerConnectionOptions = toAuthServerConnectionOptions;
        _toCampaignServerConnectionOptions = toCampaignServerConnectionOptions;
        _certificateVerificationOptions = certificateVerificationOptions;
        _intercomOptions = intercomOptions;

        Name = nameof(WebSocketFromGameplayToCampaignServerCommunicator);
        ProcessPriority = (int)ProcessPriorityEnum.Communicator;
        ProcessMode = PauseModeEnum.Process;
    }

    public event Action Connected = delegate {};

    public override void _Ready()
    {
        _wsClient.VerifySsl = _certificateVerificationOptions.VerifyCertificate;
        _wsClient.Connect("connection_closed", new Callable(this, nameof(OnConnectionClosed)));
        _wsClient.Connect("connection_error", new Callable(this, nameof(OnConnectionError)));
        _wsClient.Connect("connection_established", new Callable(this, nameof(OnConnectionEstablished)));
        _wsClient.Connect("data_received", new Callable(this, nameof(OnDataReceived)));

        AddChild(_httpRequest);
        _httpRequest.Connect("request_completed", new Callable(this, nameof(OnAuthRequestCompleted)));
    }

    public override void _PhysicsProcess(float delta)
    {
        // Server polls in _PhysicsProcess so that simulation code only runs on physics ticks
        if (_sideOptions.Side == Side.ShardServer)
            _wsClient.Poll();
    }

    public override void _Process(float delta)
    {
        if (_sideOptions.Side == Side.ShardServer && _status == Status.Disconnected)
            ConnectAsShardServer();

        // Client polls in _Process to minimize latency
        if (_sideOptions.Side == Side.Client)
            _wsClient.Poll();
    }

    public void OnConnectionClosed(bool wasCleanClose)
    {
        _status = Status.Disconnected;
        if (_sideOptions.Side == Side.Client)
            _currentUserIdRepository.Id = null;
    }

    public void OnConnectionError()
    {
// Unreachable code detected
#pragma warning disable CS0162

        throw new Exception("WebSocket connection error");
        _status = Status.Disconnected;
    }

    public void OnConnectionEstablished(string protocol)
    {
        SendPacket(new CampaignServerHandshakePacket { Token = _token.Required, Version = Const.Version });
        _token = null;
    }

    public void OnDataReceived()
    {
        byte[] bytes = _wsClient.GetPeer(1).GetPacket();
        Packet packet = _packetSerializer.Deserialize(bytes);

        if (packet is CampaignServerHandshakeAckPacket)
        {
            _status = Status.Connected;
            Connected();
        }
        else
        {
            _packetHandler.HandleAsync(packet, Const.CampaignServerId).CollectException();
        }
    }

    public void ConnectAsPlayer(string email, string password)
    {
        if (_sideOptions.Side == Side.ShardServer) throw new InvalidOperationException();
        if (_status != Status.Disconnected) return;
        _status = Status.Connecting;
        string[] headers = ["Content-Type: application/x-www-form-urlencoded"];
        string body = $"email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}";
        string url = $"{_toAuthServerConnectionOptions.AuthServerUrl}/token";
        _httpRequest.Request
        (
            url,
            method: HTTPClient.Method.Post,
            customHeaders: headers,
            requestData: body,
            sslValidateDomain: _certificateVerificationOptions.VerifyCertificate
        );
    }

    public void ConnectAsShardServer()
    {
        if (_sideOptions.Side == Side.Client) throw new InvalidOperationException();
        if (_status != Status.Disconnected) return;

        _status = Status.Connecting;
        string[] headers = ["Content-Type: application/x-www-form-urlencoded"];
        Guid id = _currentUserIdRepository.Id.Required;
        string body = $"id={Uri.EscapeDataString(id.ToString())}&role=shard" +
            $"&intercomSecret={Uri.EscapeDataString(_intercomOptions.Value.Required.IntercomSecret)}";
        string url = $"{_toAuthServerConnectionOptions.AuthServerUrl}/token/service";
        _httpRequest.Request
        (
            url,
            method: HTTPClient.Method.Post,
            customHeaders: headers,
            requestData: body,
            sslValidateDomain: _certificateVerificationOptions.VerifyCertificate
        );
    }

    public void OnAuthRequestCompleted(int result, int responseCode, string[] headers, byte[] body)
    {
        if (result != (int)HTTPRequest.Result.Success)
        {
            throw new Exception($"Authentication error: {(HTTPRequest.Result)result}");
            _status = Status.Disconnected;
        }
        else if (responseCode == 401)
        {
            throw new Exception("Incorrect credentials");
            _status = Status.Disconnected;
        }
        else if (responseCode is not (>= 200 and < 300))
        {
            throw new Exception($"Auth server responded with code {responseCode}");
            _status = Status.Disconnected;
        }
        else
        {
            _token = Encoding.UTF8.GetString(body);
            _currentUserIdRepository.Id = GetPlayerIdFromTrustedToken(_token);
            _wsClient.ConnectToUrl(_toCampaignServerConnectionOptions.CampaignServerUrl);
        }
    }

    private Guid GetPlayerIdFromTrustedToken(string token)
    {
        var claims = new JwtBuilder().DoNotVerifySignature().Decode<Dictionary<string, object>>(token);
        return Guid.Parse((string)claims["sub"]);
    }

    public void SendPacket(Packet packet)
    {
        byte[] bytes = _packetSerializer.Serialize(packet);
        _wsClient.GetPeer(1).PutPacket(bytes).ThrowIfError();
    }

    private enum Status { Disconnected, Connecting, Connected }
}
