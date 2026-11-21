namespace Soteo.Core.Snapshots;

public sealed class ShardSnapshotArchive
{
    private static readonly double SnapshotLifetime = 10;

    private readonly ICurrentTickRepository _currentTickRepo;
    private readonly IEntitySnapshotManager _entitySnapshotManager;

    private readonly IndexedShardSnapshot[] _snapshotRing =
        new IndexedShardSnapshot[Maths.CeilToInt(SnapshotLifetime * Const.TicksPerSecond)];
    private long? _lastSnapshotTick;

    public ShardSnapshotArchive
    (
        IProcessPublisher processPublisher,
        ICurrentTickRepository currentTickRepo,
        IEntitySnapshotManager entitySnapshotManager
    )
    {
        _currentTickRepo = currentTickRepo;
        _entitySnapshotManager = entitySnapshotManager;

        processPublisher.SubscribeToPhysicsProcess(Tick, ProcessPriority.SnapshotArchive, callWhenPaused: false);
    }

    private void Tick()
    {
        IndexedShardSnapshot indexedSnapshot = new IndexedShardSnapshot
        (
            new ShardSnapshot
            {
                Tick = _currentTickRepo.Tick,
                Entities = _entitySnapshotManager.GetArchiveEntitySnapshots()
            }
        );
        _snapshotRing.RingSet(_currentTickRepo.Tick, indexedSnapshot);
        _lastSnapshotTick = _currentTickRepo.Tick;
    }

    public IndexedShardSnapshot? this[long tick]
    {
        get
        {
            bool isInvalid =
                tick < 0 ||
                tick > _lastSnapshotTick ||
                tick <= _lastSnapshotTick - _snapshotRing.Length;
            return isInvalid ? null : _snapshotRing.RingGet(tick);
        }
    }
}
