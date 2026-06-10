using Soteo.Core.Gameplay.Dto;

namespace Soteo.Core.Gameplay.Abilities;

public sealed class MeleeAttackAbility : AttackAbility
{
    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.User.DealAttackDamageTo(context.TargetUnit.Required, this);
    }
}
