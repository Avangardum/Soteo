using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class PointOrUnitVectorTargetedAbility(Unit owner) : Ability(owner)
{
    public abstract bool IsValidTarget(Unit target);
    public abstract void Cast(Vector2 target, Vector2 direction);
    public abstract void Cast(Unit target, Vector2 direction);
}