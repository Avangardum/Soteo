using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Abilities;
using Soteo.Core.Entities;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Dto;

public sealed record DeflatedAbilityContext
{
    public required Ability Ability { get; init; }
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
            Ability = Ability,
            Level = Level,
            User = entityManager.GetEntity<Unit>(UserId).Required,
            UserStats = UserStats,
            ServiceProvider = serviceProvider,
            TargetPosition = TargetPosition,
            TargetUnit = TargetUnitId == null ? null : entityManager.GetEntity<Unit>(TargetUnitId.Value).Required,
            TargetDirection = TargetDirection,
            TargetShardId = TargetShardId
        };
    }
}
