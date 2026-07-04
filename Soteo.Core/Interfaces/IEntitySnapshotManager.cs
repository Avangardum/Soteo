using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.Interfaces;

public interface IEntitySnapshotManager
{
    IReadOnlyDictionary<Guid, EntitySnapshot> GetEntitySnapshots();
    IReadOnlyDictionary<Guid, EntitySnapshot> GetEntityPuppetSnapshots();
    void ReplicateEntitySnapshots(IReadOnlyDictionary<Guid, EntitySnapshot> snapshot);
    void ApplyDelta(ShardSnapshotDelta delta, double lerpWeight);
}
