namespace Soteo.Gameplay.Abilities;

public sealed class MeleeAttack : Attack<MeleeAttack>
{
    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.User.DealAttackDamageTo(context.TargetUnit!, this);
    }
}