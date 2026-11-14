using System.Diagnostics.CodeAnalysis;

namespace Soteo.Core.Entities.Projectiles;

public union ProjectileTarget(Unit, Vector2)
{
    public Vector2 Position => this switch { Unit u => u.Position, Vector2 v => v };

    public ProjectileTargetSnapshot ToSnapshot()
    {
        return this switch
        {
            Unit u => u.Id,
            Vector2 v => v
        };
    }

    public static ProjectileTarget FromSnapshot(ProjectileTargetSnapshot snapshot, IEntityManager entityManager)
    {
        return snapshot.IsUnit ?
            entityManager.GetEntity<Unit>(snapshot.UnitId.Value).Required :
            snapshot.Position.Required;
    }
}
