using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Util;

namespace Soteo.Gameplay.Abilities;

public sealed class HealAbility : Ability
{
    public override int MaxLevel => 4;
    public override Scalable<double> StaticManaCost => [100, 120, 160, 180];
    public Scalable<double> Heal => [200, 300, 400, 500];
    public override Scalable<double> StaticCooldown => [15, 13, 11, 9];
    public override Scalable<double> StaticUseTime => 0.5;
    public override Scalable<double> StaticRange => 300;
    public override CanTarget Targeting => CanTarget.Ally | CanTarget.Character;

    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.TargetUnit!.RestoreHealth(Heal[context.Level], context.User, this);
    }
}