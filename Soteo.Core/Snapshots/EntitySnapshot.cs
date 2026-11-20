using Soteo.Core.Deltas;

namespace Soteo.Core.Snapshots;

public abstract record EntitySnapshot
{
    public required Guid Id { get; init; }
    public required bool IsRemoved { get; init; }
    public required Vector2 Position { get; init; }
    public required double Azimuth { get; init; }

    public abstract EntitySnapshot ToPuppet();

    public abstract EntityDelta DeltaFrom(EntitySnapshot? from);
}

public abstract record EntitySnapshot<T> : EntitySnapshot where T : EntitySnapshot<T>
{
    public abstract EntityDelta DeltaFrom(T? from);

    public sealed override EntityDelta DeltaFrom(EntitySnapshot? from) => DeltaFrom((T?)from);
}
