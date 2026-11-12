using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Core.Abilities;

public sealed class RangedAttackAbility : AttackAbility
{
    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        double projectileSpeed = context.UserStats[Stat.AttackProjectileSpeed];
        context.GetRequiredService<IEntityManager>()
            .SpawnProjectile(context, projectileSpeed, context.TargetUnit.Required);
    }

    public override void OnProjectileHit(AbilityContext context)
    {
        base.OnProjectileHit(context);
        context.User.DealAttackDamageTo(context.TargetUnit.Required, this);
    }
}
