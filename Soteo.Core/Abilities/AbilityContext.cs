using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Core.Abilities;

public sealed record AbilityContext : ISourceUnitAndAbility
{
    public required Ability Ability { get; init; }
    public required Unit User { get; init; }
    public bool Alt { get; init; }

    /// <summary>
    /// Snapshot of user stats at the moment the ability took effect. Use this instead of User.Stats.
    /// </summary>
    public required IReadOnlyDictionary<Stat, double> UserStats { get; init; }

    public required Services Services { get; init; }
    public Vector2? TargetPosition { get; init; }
    public Unit? TargetUnit { get; init; }
    public Vector2? TargetDirection { get; init; }
    public Guid? TargetShardId { get; init; }

    Unit ISourceUnitAndAbility.Unit => User;
    AbilityContext ISourceUnitAndAbility.AbilityContext => this;

    public AbilityContextSnapshot ToSnapshot()
    {
        return new AbilityContextSnapshot
        {
            Ability = Ability,
            UserId = User.Id,
            Alt = Alt,
            UserStats = UserStats,
            TargetPosition = TargetPosition,
            TargetUnitId = TargetUnit?.Id,
            TargetDirection = TargetDirection,
            TargetShardId = TargetShardId
        };
    }

    public static AbilityContext FromSnapshot(AbilityContextSnapshot snapshot, Services services)
    {
        return new AbilityContext
        {
            Ability = snapshot.Ability,
            User = services.EntityManager.GetEntity<Unit>(snapshot.UserId).Required,
            Alt = snapshot.Alt,
            UserStats = snapshot.UserStats,
            Services = services,
            TargetPosition = snapshot.TargetPosition,
            TargetUnit = snapshot.TargetUnitId == null ? null :
                services.EntityManager.GetEntity<Unit>(snapshot.TargetUnitId.Value).Required,
            TargetDirection = snapshot.TargetDirection,
            TargetShardId = snapshot.TargetShardId
        };
    }
}
