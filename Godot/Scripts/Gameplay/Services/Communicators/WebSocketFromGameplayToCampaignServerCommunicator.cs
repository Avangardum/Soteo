using System.Text;
using JWT.Builder;
using Soteo.Core;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;

namespace Soteo.Main.Gameplay.Services.Communicators;

public sealed class WebSocketFromGameplayToCampaignServerCommunicator :
    Node, IFromGameplayToCampaignServerPacketSender, ICampaignServerConnector
{
    private const string CampaignServerUrl = "wss://localhost:3706";
    private const string AuthServerUrl = "https://localhost:3705";

    private readonly WebSocketClient _wsClient = new();
    private readonly HTTPRequest _httpRequest = new() { Name = "AuthHttpRequest", Timeout = 5 };
    
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketHandler _packetHandler;
    private readonly ICurrentUserIdRepository _currentUserIdRepository;
    private readonly ISideDetector _sideDetector;
    
    private string? _token;
    private Status _status;

    public WebSocketFromGameplayToCampaignServerCommunicator
    (
        IPacketHandler packetHandler,
        IPacketSerializer packetSerializer,
        ICurrentUserIdRepository currentUserIdRepository,
        ISideDetector sideDetector
    )
    {
        _packetHandler = packetHandler;
        _packetSerializer = packetSerializer;
        _sideDetector = sideDetector;
        _currentUserIdRepository = currentUserIdRepository;
        
        Name = nameof(WebSocketFromGameplayToCampaignServerCommunicator);
        ProcessPriority = (int)ProcessPriorityEnum.Communicator;
        PauseMode = PauseModeEnum.Process;
    }
    
    public event Action Connected = delegate {};
    
    public override void _Ready()
    {
        ProcessPriority = (int)ProcessPriorityEnum.Communicator;
        
        _wsClient.Connect("connection_closed", this, nameof(OnConnectionClosed));
        _wsClient.Connect("connection_error", this, nameof(OnConnectionError));
        _wsClient.Connect("connection_established", this, nameof(OnConnectionEstablished));
        _wsClient.Connect("data_received", this, nameof(OnDataReceived));
        
        AddChild(_httpRequest);
        _httpRequest.Connect("request_completed", this, nameof(OnAuthRequestCompleted));
    }

    public override void _PhysicsProcess(float delta)
    {
        // Server polls in _PhysicsProcess so that simulation code only runs on physics ticks
        if (_sideDetector.IsServer)
            _wsClient.Poll();
    }
    
    public override void _Process(float delta)
    {
        if (_sideDetector.IsServer && _status == Status.Disconnected)
            ConnectAsShardServer();
        
        // Client polls in _Process to minimize latency
        if (_sideDetector.IsClient)
            _wsClient.Poll();
    }

    public void OnConnectionClosed(bool wasCleanClose)
    {
        _status = Status.Disconnected;
        if (_sideDetector.IsClient)
            _currentUserIdRepository.Value = null;
    }
    
    public void OnConnectionError()
    {
        _status = Status.Disconnected;
    }
    
    public void OnConnectionEstablished(string protocol)
    {
        _status = Status.Connected;
        SendPacket(new CampaignServerHandshakePacket { Token = _token.Required, Version = Const.Version });
        _token = null;
        Connected();
    }
    
    public void OnDataReceived()
    {
        byte[] bytes = _wsClient.GetPeer(1).GetPacket();
        Packet packet = _packetSerializer.Deserialize(bytes);
        _packetHandler.HandleAsync(packet, Const.CampaignServerId).CollectException();
    }
    
    public void ConnectAsPlayer(string email, string password)
    {
        if (_sideDetector.IsServer) throw new InvalidOperationException();
        if (_status != Status.Disconnected) return;
        _status = Status.Connecting;
        string[] headers = ["Content-Type: application/x-www-form-urlencoded"];
        string body = $"email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}";
        string url = $"{AuthServerUrl}/token";
        _httpRequest.Request(url, method: HTTPClient.Method.Post, customHeaders: headers, requestData: body);
    }
    
    public void ConnectAsShardServer()
    {
        if (_sideDetector.IsClient) throw new InvalidOperationException();
        if (_status != Status.Disconnected) return;
        
        _status = Status.Connecting;
        string[] headers = ["Content-Type: application/x-www-form-urlencoded"];
        string intercomSecret = SysEnvironment.GetEnvironmentVariable("Soteo__IntercomSecret") ??
            throw new Exception("Intercom secret is not set.");
        Guid id = _currentUserIdRepository.Required;
        string body = $"id={Uri.EscapeDataString(id.ToString())}&role=shard" +
            $"&intercomSecret={Uri.EscapeDataString(intercomSecret)}";
        string url = $"{AuthServerUrl}/token/service";
        _httpRequest.Request(url, method: HTTPClient.Method.Post, customHeaders: headers, requestData: body);
    }
    
    public void OnAuthRequestCompleted(int result, int responseCode, string[] headers, byte[] body)
    {
        if (result != (int)HTTPRequest.Result.Success)
        {
            GD.PrintErr($"Authentication error: {(HTTPRequest.Result)result}");
            _status = Status.Disconnected;
        }
        else if (responseCode == 401)
        {
            GD.PrintErr("Incorrect credentials");
            _status = Status.Disconnected;
        }
        else if (responseCode is not (>= 200 and < 300))
        {
            GD.PrintErr($"Auth server responded with code {responseCode}");
            _status = Status.Disconnected;
        }
        else
        {
            _token = Encoding.UTF8.GetString(body);
            _currentUserIdRepository.Value = GetPlayerIdFromTrustedToken(_token);
            _wsClient.ConnectToUrl(CampaignServerUrl);
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
        _wsClient.GetPeer(1).PutPacket(bytes);
    }
    
    private enum Status { Disconnected, Connecting, Connected }
}
