using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Abilities;

public sealed class BloodSacrificeAbility : UntargetedAbility<BloodSacrificeAbility>
{
    public override Scalable<float> HealthCost => [100, 120, 140, 160];
    public override Scalable<float> CastTimeSeconds => 0.5f;
    private Scalable<float> ManaRestored => 200;
    public override void Cast(AbilityCastContext context) =>
        context.Caster.RestoreMana(ManaRestored[context.Level], context.Caster, this);
}