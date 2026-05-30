using Soteo.Core.Gameplay.Dto.Snapshots;

namespace Soteo.Core.Gameplay.Interfaces;

internal interface ISnapshottableEntity : IEntity
{
    EntitySnapshot CreateSnapshot();
    void ReplicateSnapshot(EntitySnapshot snapshot);
    void ApplyDelta(EntitySnapshotDelta delta, double interpolationWeight);
}
