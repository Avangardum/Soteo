using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.Interfaces;

internal interface ISnapshottableEntity : IEntity
{
    EntitySnapshot ToSnapshot();
    void ReplicateSnapshot(EntitySnapshot snapshot);
    void ApplyDelta(EntitySnapshotDelta delta, double interpolationWeight);
}
