using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.Interfaces;

public interface IShardPersistenceSnapshotManager
{
    ShardSnapshot CreateSnapshot();
    void ReplicateSnapshot(ShardSnapshot snapshot);
}
