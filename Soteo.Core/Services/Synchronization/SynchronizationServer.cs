using Soteo.Core.Dto.Packets;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;

namespace Soteo.Core.Services.Synchronization;

public sealed class SynchronizationServer : ISynchronizationServer, IDisposable // todo rename (+client)
{
    private readonly IEntitySnapshotManager _entitySnapshotManager;
    private readonly IFromGameplayPacketSender _packetSender;
    private readonly IConnectionNotifier _connectionNotifier;
    private readonly IFrameStopwatch _frameStopwatch;
    private readonly IPauseRepository _pauseRepo;
    private readonly ICurrentTickRepository _tickRepo;
    private readonly IInitializationRepository _initRepo;

    private ShardSnapshot? _prevShardSnapshot;
    private readonly HashSet<Guid> _snapshotRequesters = [];
    private readonly IDisposable _physicsProcessSubscription;
    
    public SynchronizationServer
    (
        IEntitySnapshotManager entitySnapshotManager,
        IFromGameplayPacketSender packetSender,
        IConnectionNotifier connectionNotifier,
        IProcessPublisher processPublisher,
        IFrameStopwatch frameStopwatch,
        IPauseRepository pauseRepo,
        ICurrentTickRepository tickRepo,
        IInitializationRepository initRepo
    )
    {
        _entitySnapshotManager = entitySnapshotManager;
        _packetSender = packetSender;
        _connectionNotifier = connectionNotifier;
        _frameStopwatch = frameStopwatch;
        _pauseRepo = pauseRepo;
        _tickRepo = tickRepo;
        _initRepo = initRepo;
        
        connectionNotifier.PeerConnected += OnPeerConnected;
        _physicsProcessSubscription = processPublisher
            .SubscribeToPhysicsProcess(Tick, ProcessPriorityEnum.SynchronizationServer, callWhenPaused: true);
    }

    public void Dispose()
    {
        _connectionNotifier.PeerConnected -= OnPeerConnected;
        _physicsProcessSubscription.Dispose();
    }
    
    private void OnPeerConnected(Guid peerId)
    {
        if (peerId != Const.CampaignServerId)
            _snapshotRequesters.Add(peerId);
    }

    public void Tick(double delta)
    {
        if (!_initRepo.Initialized) return;
        
        IReadOnlyDictionary<Guid, EntitySnapshot> entitySnapshots =
            _entitySnapshotManager.CreateEntityPuppetSnapshots();
        var shardSnapshot = new ShardSnapshot { Tick = _tickRepo.Value, Entities = entitySnapshots };

        if (_snapshotRequesters.Count > 0)
        {
            var shardSnapshotPacket = new ShardSnapshotPacket { Snapshot = shardSnapshot };
            _packetSender.SendReliable(shardSnapshotPacket, _snapshotRequesters);
            _snapshotRequesters.Clear();
        }
        
        if (_pauseRepo.IsPaused) return;
        
        ShardSnapshotDelta? shardSnapshotDelta = _prevShardSnapshot == null ? null :
            ShardSnapshotDelta.Between(_prevShardSnapshot, shardSnapshot);
        
        if (shardSnapshotDelta != null)
        {
            var shardSnapshotDeltaPacket = new ShardSnapshotDeltaPacket
            {
                ServerLoad = _frameStopwatch.ElapsedSincePhysicsProcess * Const.TicksPerSecond,
                SnapshotDelta = shardSnapshotDelta
            };
            _packetSender.BroadcastReliable(shardSnapshotDeltaPacket);
        }
        
        _prevShardSnapshot = shardSnapshot;
        _tickRepo.Value++;
    }

    public void ReceiveSnapshotRequest(Guid clientId) => _snapshotRequesters.Add(clientId);
}
