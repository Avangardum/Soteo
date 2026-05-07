using Soteo.Gameplay.Dto.Snapshots;

namespace Soteo.Gameplay.Interfaces;

public interface IPuppetEntitySnapshot
{
    public EntitySnapshotDelta DeltaFrom(IPuppetEntitySnapshot? from);
}