namespace Soteo.Core.Abilities;

public sealed class MeleeAttackAbility : AttackAbility
{
    protected override void TakeEffectCore(AbilityContext context)
    {
        context.User.DealAttackDamageTo(context.TargetUnit.Required, this);
    }
}
