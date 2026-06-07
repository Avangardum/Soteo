using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Shared.Dto.Snapshots;

namespace Soteo.Core.Gameplay.Interfaces;

internal interface ISnapshottableEntity : IEntity
{
    EntitySnapshot CreateSnapshot();
    void ReplicateSnapshot(EntitySnapshot snapshot);
    void ApplyDelta(EntitySnapshotDelta delta, double interpolationWeight);
}
