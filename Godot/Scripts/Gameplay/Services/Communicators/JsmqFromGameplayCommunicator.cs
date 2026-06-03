using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Extensions;
using Soteo.Util;

namespace Soteo.Gameplay.Services.Communicators;

/// <summary>
/// Communicator using the JavaScript message queue instead of WebSockets / WebRTC. Used for singleplayer in browser.
/// </summary>
public sealed class JsmqFromGameplayCommunicator :
    Node,
    IShardServerConnector,
    ICampaignServerConnector,
    IPacketSender,
    ICampaignServerPacketSender,
    INetworkDebugger,
    IConnectionNotifier
{
    private readonly ICurrentUserIdRepository _currentUserIdRepository;
    private readonly IPacketSerializer _packetSerializer;
    private readonly IPacketHandler _packetHandler;
    
    public event Action Connected = delegate { };
    public event Action<Guid> PeerConnected = delegate { };
    public event Action<Guid> PeerDisconnected = delegate { };

    private readonly HashSet<Guid> _connectedPeers = [];
    
    public JsmqFromGameplayCommunicator
    (
        ICurrentUserIdRepository currentUserIdRepository,
        IPacketSerializer packetSerializer,
        IPacketHandler packetHandler
    )
    {
        _currentUserIdRepository = currentUserIdRepository;
        _packetSerializer = packetSerializer;
        _packetHandler = packetHandler;
        
        Name = nameof(JsmqFromGameplayCommunicator);
    }

    public override void _Ready()
    {
        ProcessPriority = (int)ProcessPriorityEnum.Communicator;
        if (Const.IsServer) ConnectAsShardServer();
    }

    public override void _Process(float delta)
    {
        // Client polls in _Process to minimize latency
        if (!Const.IsServer) Poll();
    }

    public override void _PhysicsProcess(float delta)
    {
        // Server polls in _PhysicsProcess so that simulation code only runs on physics ticks
        if (Const.IsServer) Poll();
    }

    public void ConnectAsPlayer(string email, string password)
    {
        _currentUserIdRepository.Value = Const.SingleplayerPlayerId;
        SendReliable
        (
            new CampaignServerHandshakePacket { Token = "player", Version = Const.Version },
            Const.CampaignServerId
        );
        Connected();
    }

    public void ConnectAsShardServer()
    {
        SendReliable
        (
            new CampaignServerHandshakePacket { Token = "shard", Version = Const.Version },
            Const.CampaignServerId
        );
        Connected();
    }
    
    private void Poll()
    {
        while (true)
        {
            string? base64 = (string?)JavaScript.Eval($"""jsmq.receive("{_currentUserIdRepository.Value}")""");
            if (base64 == null) return;
            byte[] bytes = Convert.FromBase64String(base64);
            var senderId = new Guid(bytes.AsSpan()[..Const.BytesInGuid].ToArray());
            if (_connectedPeers.Add(senderId))
                PeerConnected(senderId);
            Packet packet = _packetSerializer.Deserialize(bytes.AsSpan()[Const.BytesInGuid..]);
            if (packet is PingPacket) return;
            _packetHandler.HandleAsync(packet, senderId).CollectException();
        }
    }

    public void SendReliable(Packet packet, Guid receiverId)
    {
        if (_currentUserIdRepository.Value == null) return;
        byte[] bytes = [.._currentUserIdRepository.Required.ToByteArray(), .._packetSerializer.Serialize(packet)];
        string base64 = Convert.ToBase64String(bytes);
        JavaScript.Eval($"""jsmq.send("{base64}", "{receiverId}");""");
    }

    public void SendUnreliable(Packet packet, Guid receiverId) => SendReliable(packet, receiverId);

    public void SendReliable(Packet packet, IEnumerable<Guid> receiverIds)
    {
        foreach (Guid receiverId in receiverIds)
            SendReliable(packet, receiverId);
    }

    public void SendUnreliable(Packet packet, IEnumerable<Guid> receiverIds) =>
        SendReliable(packet, receiverIds);

    public void BroadcastReliable(Packet packet)
    {
        if (!Const.IsServer) throw new InvalidOperationException();
        SendReliable(packet, Const.SingleplayerPlayerId);
    }

    public void BroadcastUnreliable(Packet packet) => BroadcastReliable(packet);

    void ICampaignServerPacketSender.SendPacket(Packet packet) =>
        SendReliable(packet, Const.CampaignServerId);

    public long BytesSent => 0;

    public long BytesReceived => 0;

    public double? Ping(Guid peerId) => 0;
    
    public void ConnectToShardServer(Guid id)
    {
        if (_connectedPeers.Add(id))
            PeerConnected(id);
        
        // Send ping to trigger PeerConnected on the server
        SendReliable(new PingPacket { Id = Guid.Empty, IsResponse = false }, id);
    }
}
