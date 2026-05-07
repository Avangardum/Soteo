using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Dto.Snapshots;

public abstract record PuppetEntitySnapshot<T> : EntitySnapshot<T>, IPuppetEntitySnapshot
    where T : PuppetEntitySnapshot<T>
{
    public abstract EntitySnapshotDelta DeltaFrom(T? from);

    EntitySnapshotDelta IPuppetEntitySnapshot.DeltaFrom(IPuppetEntitySnapshot? from) => DeltaFrom((T?)from);
}