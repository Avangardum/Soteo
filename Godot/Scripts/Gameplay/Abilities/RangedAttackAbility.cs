using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Abilities;

public sealed class RangedAttackAbility : AttackAbility
{
    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        double projectileSpeed = context.UserStats[Stat.AttackProjectileSpeed];
        context.GetRequiredService<IEntityManager>().SpawnAttackProjectile(context, projectileSpeed);
    }

    public override void OnProjectileHit(AbilityContext context)
    {
        base.OnProjectileHit(context);
        context.User.DealAttackDamageTo(context.TargetUnit.Required, this);
    }
}