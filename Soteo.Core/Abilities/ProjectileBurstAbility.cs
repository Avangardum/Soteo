using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Core.Abilities;

public sealed class ProjectileBurstAbility : Ability
{
    public override Targeting Targeting => Targeting.Nothing;

    protected override void TakeEffectCore(AbilityContext context)
    {
        var entityManager = context.GetRequiredService<IEntityManager>();
        for (double azimuth = 0; azimuth < 360; azimuth += 1)
        {
            Vector2 target = context.User.Position + Maths.AzimuthToDirection(azimuth) * 100;
            double speed = 15 + 1.5 * Math.Sin(Maths.Deg2Rad(azimuth) * 20);
            entityManager.SpawnProjectile(context, speed, target);
        }
    }
}
