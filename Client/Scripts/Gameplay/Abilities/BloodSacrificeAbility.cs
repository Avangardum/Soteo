namespace Soteo.Gameplay.Abilities;

public sealed class BloodSacrificeAbility : Ability<BloodSacrificeAbility>
{
    public override Scalable<float> HealthCost => [100, 120, 140, 160];
    public override Scalable<float> CastTimeSec => 0.5f;
    private Scalable<float> ManaRestored => 200;

    public override void OnCasted(AbilityCastContext context)
    {
        base.OnCasted(context);
        context.Caster.RestoreMana(ManaRestored[context.Level], context.Caster, this);
    }
}