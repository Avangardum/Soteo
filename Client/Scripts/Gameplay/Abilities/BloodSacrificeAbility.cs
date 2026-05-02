using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Util;

namespace Soteo.Gameplay.Abilities;

public sealed class BloodSacrificeAbility : Ability
{
    public override CanTarget Targeting => CanTarget.Nothing;
    public override Scalable<double> StaticHealthCost => 100;
    public override Scalable<double> StaticUseTime => 0.5;
    private Scalable<double> ManaRestored => 200;
    public override Scalable<double> StaticCooldown => 5;

    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.User.RestoreMana(ManaRestored[context.Level], context.User, this);
    }
}