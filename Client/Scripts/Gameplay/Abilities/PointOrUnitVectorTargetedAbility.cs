using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class PointOrUnitVectorTargetedAbility<T> : Ability where T : PointOrUnitTargetedAbility<T>, new()
{
    public abstract bool IsValidTarget(AbilityCastContext context, Unit target);
    public abstract void Cast(AbilityCastContext context, Vector2 target, Vector2 direction);
    public abstract void Cast(AbilityCastContext context, Unit target, Vector2 direction);
}