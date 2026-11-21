using Soteo.Core.Deltas;
using Soteo.Core.Snapshots;

namespace Soteo.Core.Entities;

public interface IEntitySnapshotManager
{
    IReadOnlyDictionary<Guid, EntitySnapshot> GetArchiveEntitySnapshots();
    IReadOnlyDictionary<Guid, EntitySnapshot> GetPersistenceEntitySnapshots();
    IReadOnlyDictionary<Guid, EntitySnapshot> GetSynchronizationEntitySnapshots();
    void ReplicateEntitySnapshots(IReadOnlyDictionary<Guid, EntitySnapshot> snapshot);
    void ApplyDelta(ShardDelta delta, double lerpWeight);
}
