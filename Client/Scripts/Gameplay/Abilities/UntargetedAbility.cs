using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class UntargetedAbility<T> : Ability<T> where T : UntargetedAbility<T>, new()
{
    public sealed override Scalable<float> CastRange => 0;

    public abstract void Cast(AbilityCastContext context);
}