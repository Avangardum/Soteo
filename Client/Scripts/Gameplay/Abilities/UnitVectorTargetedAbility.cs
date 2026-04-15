using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class UnitVectorTargetedAbility<T> : Ability<T> where T : UnitVectorTargetedAbility<T>, new()
{
    public abstract bool IsValidTarget(AbilityCastContext context, Unit target);
    public abstract void Cast(AbilityCastContext context, Unit target, Vector2 direction);
}