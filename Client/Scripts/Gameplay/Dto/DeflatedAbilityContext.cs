using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Dto;

public sealed record DeflatedAbilityContext
{
    public required int AbilityId { get; init; }
    public required int Level { get; init; }
    public required Guid UserId { get; init; }
    public required IReadOnlyDictionary<Stat, double> UserStats { get; init; }
    public Vector2? TargetPosition { get; init; }
    public Guid? TargetUnitId { get; init; }
    public Vector2? TargetDirection { get; init; }
    public Guid? TargetShardId { get; init; }
    
    public AbilityContext Inflate(IServiceProvider serviceProvider)
    {
        var entityManager = serviceProvider.GetRequiredService<IEntityManager>();
        return new AbilityContext
        {
            Ability = Ability.All[AbilityId],
            Level = Level,
            User = entityManager.GetEntity<Unit>(UserId).Required,
            UserStats = UserStats,
            ServiceProvider = serviceProvider,
            TargetPosition = TargetPosition,
            TargetUnit = TargetUnitId == null ? null : entityManager.GetEntity<Unit>(TargetUnitId.Value),
            TargetDirection = TargetDirection,
            TargetShardId = TargetShardId
        };
    }
}