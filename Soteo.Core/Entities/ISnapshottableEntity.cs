namespace Soteo.Core.Entities;

internal interface ISnapshottableEntity : IEntity // todo remove
{
    EntitySnapshot ToSnapshot();
    void ReplicateSnapshot(EntitySnapshot snapshot);
    void ApplyDelta(EntitySnapshotDelta delta, double interpolationWeight);
    void Tick(double delta);
}
