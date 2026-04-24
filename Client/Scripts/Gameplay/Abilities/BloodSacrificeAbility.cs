namespace Soteo.Gameplay.Abilities;

public sealed class BloodSacrificeAbility : Ability<BloodSacrificeAbility>
{
    public override Scalable<float> StaticHealthCost => 100;
    public override Scalable<float> StaticUseTime => 0.5f;
    private Scalable<float> ManaRestored => 200;
    public override Scalable<float> StaticCooldown => 5;

    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.User.RestoreMana(ManaRestored[context.Level], context.User, this);
    }
}