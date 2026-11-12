using Soteo.Core.Entities;

namespace Soteo.Core.Abilities;

public sealed class HealAbility : Ability
{
    public override double StaticManaCost => 180;
    public double Heal => 500;
    public override double StaticCooldown => 9;
    public override double StaticUseTime => 0.5;
    public override double StaticRange => 30;
    public override Targeting Targeting => Targeting.Ally | Targeting.Character;
    public override Targeting AltTargeting => Targeting.Nothing;

    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        Unit target = context.Alt ? context.User : context.TargetUnit.Required;
        target.RestoreHealth(Heal, context.User, this);
    }
}
