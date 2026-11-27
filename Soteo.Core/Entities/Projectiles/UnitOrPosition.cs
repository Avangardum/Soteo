using Soteo.Core.Snapshots;

namespace Soteo.Core.Entities.Projectiles;

public union UnitOrPosition(Unit, Vector2)
{
    public Vector2 Position => this switch { Unit u => u.Position, Vector2 v => v };

    public UnitOrPositionSnapshot ToSnapshot()
    {
        return this switch
        {
            Unit u => u.Id,
            Vector2 v => v
        };
    }

    public static UnitOrPosition FromSnapshot
    (
        UnitOrPositionSnapshot snapshot,
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
