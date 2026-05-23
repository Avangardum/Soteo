using Soteo.Gameplay.Dto;

namespace Soteo.Gameplay.Abilities;

public sealed class MeleeAttackAbility : AttackAbility
{
    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.User.DealAttackDamageTo(context.TargetUnit!, this);
    }
}