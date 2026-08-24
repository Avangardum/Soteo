using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.Interfaces;

public interface IShardPersistenceSnapshotManager
{
    event Action SnapshotReplicated;
    ShardSnapshot CreateSnapshot();
    void ReplicateSnapshot(ShardSnapshot snapshot);
}
