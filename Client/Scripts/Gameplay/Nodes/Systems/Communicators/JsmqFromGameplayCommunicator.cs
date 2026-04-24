using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Nodes.Systems.Communicators;

/// <summary>
/// Communicator using the JavaScript message queue instead of WebSockets / WebRTC. Used for singleplayer in browser.
/// </summary>
public sealed class JsmqFromGameplayCommunicator : Node, IMasterServerCommunicator, IPacketSender, IPingMeasurer
{
    private readonly ICurrentUserIdRepository _currentUserIdRepository;
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketHandler _packetHandler;
    private readonly IShardLoader _shardLoader;
    
    public event Action ConnectionEstablished = delegate {};
    
    public JsmqFromGameplayCommunicator
    (
        ICurrentUserIdRepository currentUserIdRepository,
        IPacketSerializer packetSerializer,
        IPacketHandler packetHandler,
        IShardLoader shardLoader
    )
    {
        _currentUserIdRepository = currentUserIdRepository;
        _packetSerializer = packetSerializer;
        _packetHandler = packetHandler;
        _shardLoader = shardLoader;
        
        Name = nameof(JsmqFromGameplayCommunicator);
    }

    public override void _Ready()
    {
        ProcessPriority = (int)ProcessPriorityEnum.Communicator;
        if (IsServer) ConnectAsShardServer();
    }

    public override void _Process(float delta)
    {
        // Client polls in _Process to minimize latency
        if (!IsServer) Poll();
    }

    public override void _PhysicsProcess(float delta)
    {
        // Server polls in _PhysicsProcess so that simulation code only runs on physics ticks
        if (IsServer) Poll();
    }

    public void ConnectAsPlayer(string email, string password)
    {
        _currentUserIdRepository.UserId = Const.SingleplayerPlayerId;
        SendReliable(new MasterServerHandshakePacket { Token = "player" }, MasterServerId );
        ConnectionEstablished();
        if (!IsServer)
        {
            SendReliable(new SpawnCharacterPacket { PeerId = Const.TestShardId }, MasterServerId);
            _shardLoader.LoadShard();
        }
    }

    public void ConnectAsShardServer()
    {
        SendReliable(new MasterServerHandshakePacket { Token = "shard" }, MasterServerId );
        ConnectionEstablished();
    }
    
    private void Poll()
    {
        while (true)
        {
            string? base64 = (string?)JavaScript.Eval($"""jsmq.receive("{_currentUserIdRepository.UserId}")""");
            if (base64 == null) return;
            byte[] bytes = Convert.FromBase64String(base64);
            var senderId = new Guid(bytes.AsSpan()[..Const.BytesInGuid].ToArray());
            Packet packet = _packetSerializer.Deserialize(bytes.AsSpan()[Const.BytesInGuid..]);
            _packetHandler.HandleAsync(packet, senderId).CollectException();
        }
    }

    public void SendReliable(Packet packet, Guid receiverId)
    {
        byte[] bytes = [.._currentUserIdRepository.UserId.ToByteArray(), .._packetSerializer.Serialize(packet)];
        string base64 = Convert.ToBase64String(bytes);
        JavaScript.Eval($"""jsmq.send("{base64}", "{receiverId}");""");
    }

    public void SendUnreliable(Packet packet, Guid receiverId) => SendReliable(packet, receiverId);

    public void BroadcastReliable(Packet packet)
    {
        if (!IsServer) throw new InvalidOperationException();
        SendReliable(packet, Const.SingleplayerPlayerId);
    }

    public void BroadcastUnreliable(Packet packet) => BroadcastReliable(packet);

    void IMasterServerCommunicator.SendPacket(Packet packet) => SendReliable(packet, MasterServerId);
    
    public float? Ping(Guid peerId) => 0;
}