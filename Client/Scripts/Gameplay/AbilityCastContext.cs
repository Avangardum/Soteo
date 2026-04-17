using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay;

public sealed record AbilityCastContext(int Level, Unit Caster, float Cooldown, IServiceProvider Services)
    : IServiceProvider
{
    public Vector2? TargetPoint { get; init; }
    public Unit? TargetUnit { get; init; }
    public Vector2? TargetDirection { get; init; }
    public Guid? TargetShardId { get; init; }
    
    public object GetService(Type serviceType) => Services.GetService(serviceType);
}