namespace Soteo.Gameplay.Abilities;

public sealed class MeleeAttack : Attack<MeleeAttack>
{
    public override void TakeEffect(AbilityUseContext context)
    {
        base.TakeEffect(context);
        context.User.DealAttackDamageTo(context.TargetUnit!, this);
    }
}