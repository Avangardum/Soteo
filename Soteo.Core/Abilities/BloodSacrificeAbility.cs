using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Enums;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Abilities;

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
