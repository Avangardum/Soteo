namespace Soteo.Core.Abilities;

public sealed class MeleeAttackAbility : AttackAbility
{
    protected override void TakeEffectCore(AbilityContext context)
    {
        context.User.DealAttackDamageTo(context.Target.Unit.Required(), this);
    }
}
