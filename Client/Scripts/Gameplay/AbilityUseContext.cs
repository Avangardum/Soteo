using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay;

public sealed record AbilityUseContext : IServiceProvider
{
    public required int Level { get; init; }
    public required Unit Caster { get; init; }
    public required IServiceProvider ServiceProvider { get; init; }
    public Vector2? TargetPosition { get; init; }
    public Unit? TargetUnit { get; init; }
    public Vector2? TargetDirection { get; init; }
    public Guid? TargetShardId { get; init; }
    
    public object? GetService(Type serviceType) => ServiceProvider.GetService(serviceType);
}