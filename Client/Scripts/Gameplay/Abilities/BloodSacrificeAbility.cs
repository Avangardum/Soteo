using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public sealed class BloodSacrificeAbility(Unit owner) : UntargetedAbility(owner)
{
    public override int HealthCost => 100;
    public override float CastTimeSeconds => 0.5f;

    public override void Cast() => owner.CurrentMana += 200;
}