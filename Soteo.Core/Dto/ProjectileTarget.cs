using System.Diagnostics.CodeAnalysis;
using System.Numerics;
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
    
    public DeflatedProjectileTarget Deflate() => IsUnit ? Unit.Id : Position;
}
