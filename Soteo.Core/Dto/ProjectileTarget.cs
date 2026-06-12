using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Dto;

public class ProjectileTarget
{
    public IUnit? Unit { get; }
    public Vector2? Position { get; }
        
    public ProjectileTarget(IUnit unit) => Unit = unit;
    public ProjectileTarget(Vector2 position) => Position = position;
        
    public static implicit operator ProjectileTarget(Vector2 target) => new(target);
        
    [MemberNotNullWhen(true, nameof(Unit))]
    [MemberNotNullWhen(false, nameof(Position))]
    public bool IsUnit => Unit != null;
    
    public ProjectileTargetSnapshot ToSnapshot() => IsUnit ? Unit.Id : Position;
    
    public static ProjectileTarget FromSnapshot(ProjectileTargetSnapshot snapshot, IServiceProvider serviceProvider)
    {
        if (snapshot.IsUnit)
        {
            var entityManager = serviceProvider.GetRequiredService<IEntityManager>();
            var target = entityManager.GetEntity<IUnit>(snapshot.UnitId.Value).Required;
            return new ProjectileTarget(target);
        }
        return snapshot.Position;
    }
}
