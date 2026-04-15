using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class UnitTargetedAbility<T> : Ability<T> where T : UnitTargetedAbility<T>, new()
{
    public abstract bool IsValidTarget(AbilityCastContext context, Unit target);
    public abstract void Cast(AbilityCastContext context, Unit target);
}