namespace Soteo.Gameplay.Abilities;

public sealed class BloodSacrificeAbility : Ability<BloodSacrificeAbility>
{
    public override Scalable<float> HealthCost => 100;
    public override Scalable<float> UseTime => 0.5f;
    private Scalable<float> ManaRestored => 200;
    public override Scalable<float> Cooldown => 5;

    public override void TakeEffect(AbilityUseContext context)
    {
        base.TakeEffect(context);
        context.Caster.RestoreMana(ManaRestored[context.Level], context.Caster, this);
    }
}