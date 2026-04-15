using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class PointTargetedAbility(Unit owner) : Ability(owner)
{
    public abstract void Cast(Vector2 target);
}