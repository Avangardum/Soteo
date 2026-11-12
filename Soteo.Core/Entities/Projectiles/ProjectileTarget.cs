using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Core.Entities.Projectiles;

public class ProjectileTarget
{
    public Unit? Unit { get; }
    public Vector2? Position { get; }

    public ProjectileTarget(Unit unit) => Unit = unit;
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
            var target = entityManager.GetEntity<Unit>(snapshot.UnitId.Value).Required;
            return new ProjectileTarget(target);
        }
        return snapshot.Position;
    }
}
