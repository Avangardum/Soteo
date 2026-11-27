using Soteo.Core.Snapshots;

namespace Soteo.Core.Abilities;

public sealed record AbilityTarget
{
    public static readonly AbilityTarget Nothing = new();

    public Vector2? Position { get; init; }
    public Unit? Unit { get; init; }
    public Vector2? Direction { get; init; }
    public Guid? ShardId { get; init; }

    public AbilityTargetSnapshot ToSnapshot()
    {
        if (this == Nothing) return AbilityTargetSnapshot.Nothing;

        return new()
        {
            Position = Position,
            UnitId = Unit?.Id,
            Direction = Direction,
            ShardId = ShardId
        };
    }

    public static AbilityTarget FromSnapshot
    (
        AbilityTargetSnapshot snapshot,
        Func<Guid, Entity> replicateEntityReference
    )
    {
        if (snapshot == AbilityTargetSnapshot.Nothing) return Nothing;

        return new()
        {
            Position = snapshot.Position,
            Unit = (Unit?)snapshot.UnitId?.PassTo(replicateEntityReference),
            Direction = snapshot.Direction,
            ShardId = snapshot.ShardId
        };
    }
}
