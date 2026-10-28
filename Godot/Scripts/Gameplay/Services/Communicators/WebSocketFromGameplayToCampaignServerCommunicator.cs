using System.Text;
using JWT.Builder;
using Soteo.Core.Dto.Options;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.SidedDependencies;
using Soteo.Core.StaticHelpers;

namespace Soteo.Main.Gameplay.Services.Communicators;

public sealed partial class WebSocketFromGameplayToCampaignServerCommunicator :
    Node, IFromGameplayToCampaignServerPacketSender, ICampaignServerConnector
{
    private readonly WebSocketPeer _socket = new();
    private readonly HttpRequest _httpRequest = new() { Name = "AuthHttpRequest", Timeout = 15 };

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
        ProcessMode = ProcessModeEnum.Always;

        if (!certificateVerificationOptions.VerifyCertificate)
            _httpRequest.SetTlsOptions(TlsOptions.ClientUnsafe());
    }

    public event Action Connected = delegate {};

    public override void _Ready()
    {
        AddChild(_httpRequest);
        _httpRequest.RequestCompleted += OnAuthRequestCompleted;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Server polls in _PhysicsProcess, so that simulation code only runs on physics ticks
        if (_sideOptions.Side == Side.ShardServer)
            Poll();
    }

    public override void _Process(double delta)
    {
        if (_sideOptions.Side == Side.ShardServer && _status == Status.Disconnected)
            ConnectAsShardServer();

        // Client polls in _Process to minimize latency
        if (_sideOptions.Side == Side.Client)
            Poll();
    }

    private void Poll()
    {
        _socket.Poll();
        switch (_socket.GetReadyState())
        {
            case WebSocketPeer.State.Open:
                if (_status == Status.Connecting)
                {
                    _status = Status.Handshake;
                    _token = null;
                    SendPacket(new CampaignServerHandshakePacket { Token = _token.Required, Version = Const.Version });
                }
                else
                {
                    byte[] bytes = _socket.GetPacket();
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
                break;
            case WebSocketPeer.State.Closed:
                _status = Status.Disconnected;
                if (_sideOptions.Side == Side.Client)
                    _currentUserIdRepository.Id = null;
                break;
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
            method: Godot.HttpClient.Method.Post,
            customHeaders: headers,
            requestData: body
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
            method: Godot.HttpClient.Method.Post,
            customHeaders: headers,
            requestData: body
        );
    }

    private void OnAuthRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
    {
        if (result != (int)HttpRequest.Result.Success)
        {
            _status = Status.Disconnected;
            throw new Exception($"Authentication error: {(HttpRequest.Result)result}");
        }
        else if (responseCode == 401)
        {
            _status = Status.Disconnected;
            throw new Exception("Incorrect credentials");
        }
        else if (responseCode is not (>= 200 and < 300))
        {
            _status = Status.Disconnected;
            throw new Exception($"Auth server responded with code {responseCode}");
        }
        else
        {
            _token = Encoding.UTF8.GetString(body);
            _currentUserIdRepository.Id = GetPlayerIdFromTrustedToken(_token);
            TlsOptions tlsOptions = _certificateVerificationOptions.VerifyCertificate ?
                TlsOptions.Client() :
                TlsOptions.ClientUnsafe();
            _socket.ConnectToUrl(_toCampaignServerConnectionOptions.CampaignServerUrl, tlsOptions);
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
        _socket.PutPacket(bytes).ThrowIfError();
    }

    private enum Status { Disconnected, Connecting, Handshake, Connected }
}
