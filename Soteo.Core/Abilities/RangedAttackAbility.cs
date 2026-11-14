using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Core.Abilities;

public sealed class RangedAttackAbility : AttackAbility
{
    protected override void TakeEffectCore(AbilityContext context)
    {
        double projectileSpeed = context.UserStats[Stat.AttackProjectileSpeed];
        context.Services.EntityManager
            .SpawnProjectile(context, projectileSpeed, context.TargetUnit.Required);
    }

    public override void OnProjectileHit(AbilityContext context)
    {
        context.User.DealAttackDamageTo(context.TargetUnit.Required, this);
    }
}
