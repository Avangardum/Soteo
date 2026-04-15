using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class UnitTargetedAbility(Unit owner) : Ability(owner)
{
    public abstract bool IsValidTarget(Unit target);
    public abstract void Cast(Unit target);
}