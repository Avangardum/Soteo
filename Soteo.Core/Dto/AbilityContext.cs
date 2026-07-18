using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Abilities;
using Soteo.Core.Entities;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Dto;

public sealed record AbilityContext : IServiceProvider, ISourceUnitAndAbility
{
    public required Ability Ability { get; init; }
    public required int Level { get; init; }
    public required IUnit User { get; init; }
    
    /// <summary>
    /// Snapshot of user stats at the moment the ability took effect. Use this instead of User.Stats.
    /// </summary>
    public required IReadOnlyDictionary<Stat, double> UserStats { get; init; }
    
    public required IServiceProvider ServiceProvider { get; init; }
    public Vector2? TargetPosition { get; init; }
    public IUnit? TargetUnit { get; init; }
    public Vector2? TargetDirection { get; init; }
    public Guid? TargetShardId { get; init; }

    IUnit ISourceUnitAndAbility.Unit => User;
    AbilityContext ISourceUnitAndAbility.AbilityContext => this;
    
    public object? GetService(Type serviceType) => ServiceProvider.GetService(serviceType);
    
    public AbilityContextSnapshot ToSnapshot()
    {
        return new AbilityContextSnapshot
        {
            Ability = Ability,
            Level = Level,
            UserId = User.Id,
            UserStats = UserStats,
            TargetPosition = TargetPosition,
            TargetUnitId = TargetUnit?.Id,
            TargetDirection = TargetDirection,
            TargetShardId = TargetShardId,
        };
    }
    
    public static AbilityContext FromSnapshot(AbilityContextSnapshot snapshot, IServiceProvider serviceProvider)
    {
        var entityManager = serviceProvider.GetRequiredService<IEntityManager>();
        return new AbilityContext
        {
            Ability = snapshot.Ability,
            Level = snapshot.Level,
            User = entityManager.GetEntity<Unit>(snapshot.UserId).Required,
            UserStats = snapshot.UserStats,
            ServiceProvider = serviceProvider,
            TargetPosition = snapshot.TargetPosition,
            TargetUnit = snapshot.TargetUnitId == null ? null :
                entityManager.GetEntity<Unit>(snapshot.TargetUnitId.Value).Required,
            TargetDirection = snapshot.TargetDirection,
            TargetShardId = snapshot.TargetShardId,
        };
    }
}
