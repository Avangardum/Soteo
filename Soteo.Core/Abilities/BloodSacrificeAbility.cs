namespace Soteo.Core.Abilities;

public sealed class BloodSacrificeAbility : Ability
{
    public override Targeting Targeting => Targeting.Nothing;
    public override double StaticHealthCost => 100;
    public override double StaticUseTime => 0.5;
    private double ManaRestored => 200;
    public override double StaticCooldown => 5;

    protected override void TakeEffectCore(AbilityContext context)
    {
        context.User.RestoreMana(ManaRestored, context.User, this);
    }
}
