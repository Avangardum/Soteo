namespace Soteo.Core.Entities;

internal interface ISnapshottableEntity : IEntity
{
    EntitySnapshot ToSnapshot();
    void ReplicateSnapshot(EntitySnapshot snapshot);
    void ApplyDelta(EntitySnapshotDelta delta, double interpolationWeight);
}
