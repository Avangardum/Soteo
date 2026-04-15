using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public sealed class BloodSacrificeAbility : UntargetedAbility<BloodSacrificeAbility>
{
    public override Scalable<int> HealthCost => [100, 120, 140, 160];
    public override Scalable<float> CastTimeSeconds => 0.5f;
    public override void Cast(AbilityCastContext context) => context.Caster.CurrentMana += 200;
}