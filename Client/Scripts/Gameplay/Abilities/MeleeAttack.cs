using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Abilities;

public sealed class MeleeAttack : Attack<MeleeAttack>
{
    public override void TakeEffect(AbilityUseContext context)
    {
        base.TakeEffect(context);
        context.TargetUnit!.DealDamage(context.User.Stats[Stat.AttackDamage], context.User, this);
    }
}