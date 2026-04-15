using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class PointVectorTargetedAbility<T> : Ability<T> where T : PointVectorTargetedAbility<T>, new()
{
    public abstract void Cast(AbilityCastContext context, Vector2 target, Vector2 direction);
}