using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Core.Entities.Projectiles;

public class ProjectileTarget // todo to union
{
    public Unit? Unit { get; }
    public Vector2? Position { get; }

    public ProjectileTarget(Unit unit) => Unit = unit;
    public ProjectileTarget(Vector2 position) => Position = position;

    public static implicit operator ProjectileTarget(Vector2 target) => new(target);
    public static implicit operator ProjectileTarget(Unit target) => new(target);

    [MemberNotNullWhen(true, nameof(Unit))]
    [MemberNotNullWhen(false, nameof(Position))]
    public bool IsUnit => Unit != null;

    public ProjectileTargetSnapshot ToSnapshot() => IsUnit ? Unit.Id : Position;

    public static ProjectileTarget FromSnapshot(ProjectileTargetSnapshot snapshot, IEntityManager entityManager)
    {
        return snapshot.IsUnit ? entityManager.GetEntity<Unit>(snapshot.UnitId.Value).Required : snapshot.Position;
    }
}
