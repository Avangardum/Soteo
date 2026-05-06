using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Entities;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Dto;

public sealed record AbilityContext : IServiceProvider
{
    public required Ability Ability { get; init; }
    public required int Level { get; init; }
    public required Unit User { get; init; }
    /// <summary>
    /// Snapshot of user stats at the moment the ability took effect. Use this instead of User.Stats.
    /// </summary>
    public required IReadOnlyDictionary<Stat, double> UserStats { get; init; }
    public required IServiceProvider ServiceProvider { get; init; }
    public Vector2? TargetPosition { get; init; }
    public Unit? TargetUnit { get; init; }
    public Vector2? TargetDirection { get; init; }
    public Guid? TargetShardId { get; init; }
    
    public object? GetService(Type serviceType) => ServiceProvider.GetService(serviceType);
    
    public DeflatedAbilityContext Deflate()
    {
        return new DeflatedAbilityContext
        {
            Ability = Ability,
            Level = Level,
            UserId = User.Id,
            UserStats = UserStats,
            TargetPosition = TargetPosition,
            TargetUnitId = TargetUnit?.Id,
            TargetDirection = TargetDirection,
            TargetShardId = TargetShardId
        };
    }
}