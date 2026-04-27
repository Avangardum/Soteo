using Soteo.Gameplay.Dto;

namespace Soteo.Gameplay.Abilities;

public sealed class MeleeAttack : Attack
{
    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.User.DealAttackDamageTo(context.TargetUnit!, this);
    }
}