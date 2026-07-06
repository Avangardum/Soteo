using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services;

public sealed class ShardPersistenceSnapshotManager
(
    IEntitySnapshotManager entitySnapshotManager,
    ICurrentTickRepository tickRepo
) : IShardPersistenceSnapshotManager
{
    public ShardSnapshot CreateSnapshot()
    {
        return new ShardSnapshot
        {
            Tick = tickRepo.Value,
            Entities = entitySnapshotManager.CreateEntitySnapshots(),
        };
    }

    public void ReplicateSnapshot(ShardSnapshot snapshot)
    {
        throw new NotImplementedException();
    }
}
