using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class UntargetedAbility(Unit owner) : Ability(owner)
{
    public sealed override float CastRange => 0;

    public abstract void Cast();
}