using Soteo.Core.Commands;
using Soteo.Core.Snapshots;

namespace Soteo.Core.Entities.Units;

public sealed record UnitAiContext
{
    private readonly Unit _unit;

    public UnitSnapshot Self { get; }
    public IndexedShardSnapshot Snapshot { get; }
    public ShardSnapshotArchive ShardSnapshotArchive { get; }

    public UnitAiContext
    (
        Unit unit,
        UnitSnapshot self,
        IndexedShardSnapshot snapshot,
        ShardSnapshotArchive shardSnapshotArchive
    )
    {
        _unit = unit;
        Self = self;
        Snapshot = snapshot;
        ShardSnapshotArchive = shardSnapshotArchive;
    }

    public void SetCommand(Command command) => _unit.SetCommand(command);
}
