namespace Soteo.Core.Abilities;

public sealed class RangedAttackAbility : AttackAbility
{
    protected override void TakeEffectCore(AbilityContext context)
    {
        double projectileSpeed = context.UserStatsAtTakeEffect[Stat.AttackProjectileSpeed];
        context.Services.EntityManager
            .SpawnProjectile(context, projectileSpeed, context.Target.Unit.Required());
    }

    public override void OnProjectileHit(AbilityContext context)
    {
        context.User.DealAttackDamageTo(context.Target.Unit.Required(), this);
    }
}
