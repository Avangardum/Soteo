using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class PointTargetedAbility<T> : Ability<T> where T : PointTargetedAbility<T>, new()
{
    public abstract void Cast(AbilityCastContext context, Vector2 target);
}