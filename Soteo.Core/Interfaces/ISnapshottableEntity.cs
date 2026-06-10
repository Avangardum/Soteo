using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.Interfaces;

internal interface ISnapshottableEntity : IEntity
{
    EntitySnapshot CreateSnapshot();
    void ReplicateSnapshot(EntitySnapshot snapshot);
    void ApplyDelta(EntitySnapshotDelta delta, double interpolationWeight);
}
