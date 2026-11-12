using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Core.Abilities;

public sealed record AbilityContext : IServiceProvider, ISourceUnitAndAbility
{
    public required Ability Ability { get; init; }
    public required Unit User { get; init; }
    public bool Alt { get; init; }

    /// <summary>
    /// Snapshot of user stats at the moment the ability took effect. Use this instead of User.Stats.
    /// </summary>
    public required IReadOnlyDictionary<Stat, double> UserStats { get; init; }

    public required IServiceProvider ServiceProvider { get; init; }
    public Vector2? TargetPosition { get; init; }
    public Unit? TargetUnit { get; init; }
    public Vector2? TargetDirection { get; init; }
    public Guid? TargetShardId { get; init; }

    Unit ISourceUnitAndAbility.Unit => User;
    AbilityContext ISourceUnitAndAbility.AbilityContext => this;

    public object? GetService(Type serviceType) => ServiceProvider.GetService(serviceType);

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

    public static AbilityContext FromSnapshot(AbilityContextSnapshot snapshot, IServiceProvider serviceProvider)
    {
        var entityManager = serviceProvider.GetRequiredService<IEntityManager>();
        return new AbilityContext
        {
            Ability = snapshot.Ability,
            User = entityManager.GetEntity<Unit>(snapshot.UserId).Required,
            Alt = snapshot.Alt,
            UserStats = snapshot.UserStats,
            ServiceProvider = serviceProvider,
            TargetPosition = snapshot.TargetPosition,
            TargetUnit = snapshot.TargetUnitId == null ? null :
                entityManager.GetEntity<Unit>(snapshot.TargetUnitId.Value).Required,
            TargetDirection = snapshot.TargetDirection,
            TargetShardId = snapshot.TargetShardId
        };
    }
}
