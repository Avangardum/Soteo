using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Shared.Dto.Snapshots;

namespace Soteo.Core.Gameplay.Interfaces;

public interface IEntitySnapshotManager
{
    IReadOnlyDictionary<Guid, EntitySnapshot> GetEntityPuppetSnapshots();
    void ReplicateEntitySnapshots(IReadOnlyDictionary<Guid, EntitySnapshot> snapshot);
    void ApplyDelta(ShardSnapshotDelta delta, double lerpWeight);
}
