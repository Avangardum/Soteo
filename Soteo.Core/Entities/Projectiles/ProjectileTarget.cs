using Soteo.Core.Snapshots;

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

    public static ProjectileTarget FromSnapshot
    (
        ProjectileTargetSnapshot snapshot,
        Func<Guid, Entity> replicateEntityReference
    )
    {
        return snapshot switch
        {
            Guid unitId => (Unit)replicateEntityReference(unitId),
            Vector2 position => position
        };
    }
}
