using Soteo.Core.Snapshots;

namespace Soteo.Core.Abilities;

public sealed record AbilityContext : ISourceUnitAndAbility
{
    public required Ability Ability { get; init; }
    public required Unit User { get; init; }
    public bool Alt { get; init; }

    public required IReadOnlyDictionary<Stat, double> UserStatsAtTakeEffect { get; init; }

    public required Services Services { get; init; }
    public AbilityTarget Target { get; init; } = AbilityTarget.Nothing;

    Unit ISourceUnitAndAbility.Unit => User;
    AbilityContext ISourceUnitAndAbility.AbilityContext => this;

    public AbilityContextSnapshot ToSnapshot()
    {
        return new AbilityContextSnapshot
        {
            Ability = Ability,
            UserId = User.Id,
            Alt = Alt,
            UserStats = UserStatsAtTakeEffect,
            Target = Target.ToSnapshot()
        };
    }

    public static AbilityContext FromSnapshot
    (
        AbilityContextSnapshot snapshot,
        Services services,
        Func<Guid, Entity> replicateEntityReference
    )
    {
        return new AbilityContext
        {
            Ability = snapshot.Ability,
            User = (Unit)replicateEntityReference(snapshot.UserId),
            Alt = snapshot.Alt,
            UserStatsAtTakeEffect = snapshot.UserStats,
            Services = services,
            Target = AbilityTarget.FromSnapshot(snapshot.Target, replicateEntityReference)
        };
    }
}
