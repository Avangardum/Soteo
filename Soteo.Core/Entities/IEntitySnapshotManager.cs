namespace Soteo.Core.Entities;

public interface IEntitySnapshotManager
{
    IReadOnlyDictionary<Guid, EntitySnapshot> CreateEntitySnapshots();
    IReadOnlyDictionary<Guid, EntitySnapshot> CreateEntityPuppetSnapshots();
    void ReplicateEntitySnapshots(IReadOnlyDictionary<Guid, EntitySnapshot> snapshot);
    void ApplyDelta(ShardSnapshotDelta delta, double lerpWeight);
}
