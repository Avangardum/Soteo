using System;
using System.Text;
using Godot;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Shared;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Packets.Master;
using Soteo.Shared.Packets.Shared;
using Soteo.Shared.PacketSerializers;

namespace Soteo.Client;

public sealed class MasterServerCommunicator : Node, IMasterServerCommunicator
{
    private enum Status { Disconnected, Connecting, Connected }
    
    private const string MasterServerUrl = "ws://localhost:3706"; // todo wss
    private const string AuthServerUrl = "http://localhost:3705"; // todo https
    
    public static MasterServerCommunicator Instance { get; private set; } = null!;
    
    private readonly WebSocketClient _wsClient = new();
    private readonly IPacketSerializer _packetSerializer = new UniversalPacketSerializer();
    private readonly HTTPRequest _httpRequest = new() { Name = "AuthHttpRequest", Timeout = 5 };
    
    private string _token = "";
    private Status _status;

    public event Action ConnectionEstablished = delegate {};
    
    public override void _Ready()
    {
        if (Instance != null) throw new InvalidOperationException();
        Instance = this;
        
        _wsClient.Connect("connection_closed", this, nameof(OnConnectionClosed));
        _wsClient.Connect("connection_error", this, nameof(OnConnectionError));
        _wsClient.Connect("connection_established", this, nameof(OnConnectionEstablished));
        _wsClient.Connect("data_received", this, nameof(OnDataReceived));
        _wsClient.Connect("server_close_request", this, nameof(OnServerCloseRequest));
        
        AddChild(_httpRequest);
        _httpRequest.Connect("request_completed", this, nameof(OnAuthRequestCompleted));
        
        if (IsServer) ConnectAsShardServer();
    }

    public override void _PhysicsProcess(float delta)
    {
        _wsClient.Poll();
    }

    public void OnConnectionClosed(bool wasCleanClose)
    {
        GD.Print("Master server connection closed");
        _status = Status.Disconnected;
    }
    
    public void OnConnectionError()
    {
        GD.Print("Master server connection error");
        _status = Status.Disconnected;
    }
    
    public void OnConnectionEstablished(string protocol)
    {
        _status = Status.Connected;
        GD.Print("Master server connection established, sending handshake packet");
        SendPacket(new HandshakePacket { Token = _token, Version = Const.Version });
        ConnectionEstablished();
        if (!IsServer)
        {
            SendPacket(new SpawnCharacterPacket { PeerId = Const.TestShardId });
            GetTree().ChangeScene("res://Scenes/Maps/Test.tscn");
        }
    }
    
    public void OnDataReceived()
    {
        byte[] bytes = _wsClient.GetPeer(1).GetPacket();
        Packet packet = _packetSerializer.Deserialize(bytes);
        IPacketHandler handler =
            (IPacketHandler)ServiceProvider.Instance.GetRequiredService(TypeLocator.PacketHandlerTypes[packet.Type]);
        handler.HandleAsync(packet, MasterServerId);
    }
    
    public void OnServerCloseRequest(int code, string reason)
    {
        GD.Print($"Master server closes connection with code {code} and reason {reason}");
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
        string intercomSecret = ClrEnvironment.GetEnvironmentVariable("Soteo__IntercomSecret") ??
            throw new InvalidOperationException("Intercom secret is not set.");
        Guid id = GetShardServerId();
        string body = $"id={Uri.EscapeDataString(id.ToString())}&role=shard" +
            $"&intercomSecret={Uri.EscapeDataString(intercomSecret)}";
        string url = $"{AuthServerUrl}/token/service";
        _httpRequest.Request(url, method: HTTPClient.Method.Post, customHeaders: headers, requestData: body);
    }
    
    private Guid GetShardServerId()
    {
        string[] args = OS.GetCmdlineArgs();
        int idIndex = args.IndexOf("--server") + 1;
        if (idIndex == 0 || idIndex == args.Length || !Guid.TryParse(args[idIndex], out var id))
            throw new ArgumentException("No server id found in args");
        return id;
    }
    
    public void OnAuthRequestCompleted(int result, int responseCode, string[] headers, byte[] body)
    {
        if (result != (int)HTTPRequest.Result.Success)
        {
            GD.PrintErr($"Authentication error: {(HTTPRequest.Result)result}");
            _status = Status.Disconnected;
        }
        else if (responseCode != 200)
        {
            GD.PrintErr("Incorrect credentials");
            _status = Status.Disconnected;
        }
        else
        {
            _token = Encoding.UTF8.GetString(body);
            _wsClient.ConnectToUrl(MasterServerUrl);
        }
    }

    public void SendPacket(Packet packet)
    {
        byte[] bytes = _packetSerializer.Serialize(packet);
        _wsClient.GetPeer(1).PutPacket(bytes);
    }
    
    void IPacketSender.SendReliable(Packet packet, Guid receiverId)
    {
        if (receiverId != MasterServerId) throw new InvalidOperationException();
        SendPacket(packet);
    }
}