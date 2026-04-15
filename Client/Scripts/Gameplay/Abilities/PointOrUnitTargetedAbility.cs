using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class PointOrUnitTargetedAbility<T> : Ability<T> where T : PointOrUnitTargetedAbility<T>, new()
{
    public abstract bool IsValidTarget(AbilityCastContext context, Unit target);
    public abstract void Cast(AbilityCastContext context, Vector2 target);
    public abstract void Cast(AbilityCastContext context, Unit target);
}