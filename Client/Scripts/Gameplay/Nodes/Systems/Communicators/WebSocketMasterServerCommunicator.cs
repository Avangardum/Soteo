using System.Text;
using JWT.Builder;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Attributes;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Packets;
using Soteo.Shared.PacketSerializers;

namespace Soteo.Gameplay.Nodes.Systems.Communicators;

public sealed class WebSocketMasterServerCommunicator : Node, IMasterServerCommunicator
{
    private enum Status { Disconnected, Connecting, Connected }
    
    private const string MasterServerUrl = "wss://localhost:3706";
    private const string AuthServerUrl = "https://localhost:3705";

    private readonly WebSocketClient _wsClient = new();
    private IPacketSerializer _packetSerializer = null!;
    private readonly HTTPRequest _httpRequest = new() { Name = "AuthHttpRequest", Timeout = 5 };
    private IPacketHandler _packetHandler = null!;
    private IShardLoader _shardLoader = null!;
    private ICurrentUserIdRepository _currentUserIdRepository = null!;
    
    private string _token = "";
    private Status _status;

    [Inject]
    public void Inject
    (
        IPacketHandler packetHandler,
        IPacketSerializer packetSerializer,
        IShardLoader shardLoader,
        ICurrentUserIdRepository currentUserIdRepository
    )
    {
        _packetHandler = packetHandler;
        _packetSerializer = packetSerializer;
        _shardLoader = shardLoader;
        _currentUserIdRepository = currentUserIdRepository;
        
        // When running the server from the editor, connect on button press
        if (IsServer && !Main.EditorIsServer) ConnectAsShardServer();
    }
    
    public event Action ConnectionEstablished = delegate {};
    
    public override void _Ready()
    {
        if (UseJsmq)
        {
            SetProcess(false);
            SetPhysicsProcess(false);
            QueueFree();
            return;
        }
        
        ProcessPriority = (int)ProcessPriorityEnum.Communicator;
        
        _wsClient.Connect("connection_closed", this, nameof(OnConnectionClosed));
        _wsClient.Connect("connection_error", this, nameof(OnConnectionError));
        _wsClient.Connect("connection_established", this, nameof(OnConnectionEstablished));
        _wsClient.Connect("data_received", this, nameof(OnDataReceived));
        _wsClient.Connect("server_close_request", this, nameof(OnServerCloseRequest));
        
        AddChild(_httpRequest);
        _httpRequest.Connect("request_completed", this, nameof(OnAuthRequestCompleted));
    }

    public override void _PhysicsProcess(float delta)
    {
        // Server polls in _PhysicsProcess so that simulation code only runs on physics ticks
        if (IsServer) _wsClient.Poll();
    }
    
    public override void _Process(float delta)
    {
        // Client polls in _Process to minimize latency
        if (!IsServer) _wsClient.Poll();
    }

    public void OnConnectionClosed(bool wasCleanClose)
    {
        _status = Status.Disconnected;
        _currentUserIdRepository.UserId = Guid.Empty;
    }
    
    public void OnConnectionError()
    {
        _status = Status.Disconnected;
    }
    
    public void OnConnectionEstablished(string protocol)
    {
        _status = Status.Connected;
        SendPacket(new MasterServerHandshakePacket { Token = _token, Version = Const.Version });
        _token = "";
        ConnectionEstablished();
        
        if (!IsServer)
        {
            SendPacket(new SpawnCharacterPacket { PeerId = Const.TestShardId });
            _shardLoader.LoadShard();
        }
    }
    
    public void OnDataReceived()
    {
        byte[] bytes = _wsClient.GetPeer(1).GetPacket();
        Packet packet = _packetSerializer.Deserialize(bytes);
        _packetHandler.HandleAsync(packet, MasterServerId).CollectException();
    }
    
    public void OnServerCloseRequest(int code, string reason)
    {
        
    }
    
    public void ConnectAsPlayer(string email, string password)
    {
        if (_status != Status.Disconnected) return;
        _status = Status.Connecting;
        string[] headers = ["Content-Type: application/x-www-form-urlencoded"];
        string body = $"email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}";
        string url = $"{AuthServerUrl}/token";
        _httpRequest.Request(url, method: HTTPClient.Method.Post, customHeaders: headers, requestData: body);
    }
    
    public void ConnectAsShardServer()
    {
        if (_status != Status.Disconnected) return;
        _status = Status.Connecting;
        string[] headers = ["Content-Type: application/x-www-form-urlencoded"];
        string intercomSecret = SysEnvironment.GetEnvironmentVariable("Soteo__IntercomSecret") ??
            throw new InvalidOperationException("Intercom secret is not set.");
        Guid id = _currentUserIdRepository.UserId;
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
        else if (responseCode / 100 != 2)
        {
            GD.PrintErr($"Auth server responded with code {responseCode}");
        }
        else
        {
            _token = Encoding.UTF8.GetString(body);
            _currentUserIdRepository.UserId = GetPlayerIdFromTrustedToken(_token);
            _wsClient.ConnectToUrl(MasterServerUrl);
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
}