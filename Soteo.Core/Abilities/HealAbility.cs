using Soteo.Core.Dto;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Abilities;

public sealed class HealAbility : Ability
{
    // todo fix broken description
    
    public override int MaxLevel => 4;
    public override Scalable<double> StaticManaCost => [100, 120, 160, 180];
    public Scalable<double> Heal => [200, 300, 400, 500];
    public override Scalable<double> StaticCooldown => [15, 13, 11, 9];
    public override Scalable<double> StaticUseTime => 0.5;
    public override Scalable<double> StaticRange => 300;
    public override Targeting Targeting => Targeting.Ally | Targeting.Character;
    public override Targeting AltTargeting => Targeting.Nothing;

    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        IUnit target = context.Alt ? context.User : context.TargetUnit.Required;
        target.RestoreHealth(Heal[context.Level], context.User, this);
    }
}
