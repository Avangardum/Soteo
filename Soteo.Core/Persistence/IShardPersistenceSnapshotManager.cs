namespace Soteo.Core.Persistence;

public interface IShardPersistenceSnapshotManager
{
    event Action SnapshotReplicated;
    ShardSnapshot CreateSnapshot();
    void ReplicateSnapshot(ShardSnapshot snapshot);
}
