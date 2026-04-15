using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class PointOrUnitTargetedAbility(Unit owner) : Ability(owner)
{
    public abstract bool IsValidTarget(Unit target);
    public abstract void Cast(Vector2 target);
    public abstract void Cast(Unit target);
}