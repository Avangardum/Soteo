using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Abilities;

public sealed class RangedAttack : Attack<RangedAttack>
{
    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.GetRequiredService<IEntityManager>().SpawnAttackProjectile(context.User, this, context.TargetUnit!,
            context.User.Stats[Stat.AttackProjectileSpeed]);
    }
}