using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class PointVectorTargetedAbility(Unit owner) : Ability(owner)
{
    public abstract void Cast(Vector2 target, Vector2 direction);
}