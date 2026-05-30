using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Core.Gameplay.Dto;

public class DeflatedProjectileTarget
{
    public Guid? UnitId { get; }
    public Vector2? Position { get; }
    
    public DeflatedProjectileTarget(Guid unitId) => UnitId = unitId;
    public DeflatedProjectileTarget(Vector2 position) => Position = position;
    
    public static implicit operator DeflatedProjectileTarget(Guid unitId) => new(unitId);
    public static implicit operator DeflatedProjectileTarget(Vector2 position) => new(position);
    
    [MemberNotNullWhen(true, nameof(UnitId))]
    [MemberNotNullWhen(false, nameof(Position))]
    public bool IsUnit => UnitId != null;
    
    public ProjectileTarget Inflate(IServiceProvider serviceProvider)
    {
        if (IsUnit)
        {
            var target = serviceProvider.GetRequiredService<IEntityManager>().GetEntity<IUnit>(UnitId.Value).Required;
            return new ProjectileTarget(target);
        }
        return Position;
    }
}
