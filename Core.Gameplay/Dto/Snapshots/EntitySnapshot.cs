using System.Numerics;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Dto.Snapshots;

public abstract record EntitySnapshot
{
    public required Guid Id { get; init; }
    public required Vector2 Position { get; init; }
    public required double Azimuth { get; init; }
    
    public virtual EntitySnapshot ToPuppet() => this; // todo abstract
    
    public abstract EntitySnapshotDelta DeltaFrom(EntitySnapshot? from);
}

public abstract record EntitySnapshot<T> : EntitySnapshot where T : EntitySnapshot<T>
{
    public abstract EntitySnapshotDelta DeltaFrom(T? from);

    public sealed override EntitySnapshotDelta DeltaFrom(EntitySnapshot? from) => DeltaFrom((T?)from);
}
