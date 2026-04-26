using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay;

public sealed record AbilityContext : IServiceProvider
{
    public sealed record Deflated
    {
        public required int AbilityId { get; init; }
        public required int Level { get; init; }
        public required Guid UserId { get; init; }
        public required IReadOnlyDictionary<Stat, float> UserStats { get; init; }
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
    
    public required Ability Ability { get; init; }
    public required int Level { get; init; }
    public required Unit User { get; init; }
    /// <summary>
    /// Snapshot of user stats at the moment the ability took effect. Use this instead of User.Stats.
    /// </summary>
    public required IReadOnlyDictionary<Stat, float> UserStats { get; init; }
    public required IServiceProvider ServiceProvider { get; init; }
    public Vector2? TargetPosition { get; init; }
    public Unit? TargetUnit { get; init; }
    public Vector2? TargetDirection { get; init; }
    public Guid? TargetShardId { get; init; }
    
    public object? GetService(Type serviceType) => ServiceProvider.GetService(serviceType);
    
    public Deflated Deflate()
    {
        return new Deflated
        {
            AbilityId = Ability.Id,
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