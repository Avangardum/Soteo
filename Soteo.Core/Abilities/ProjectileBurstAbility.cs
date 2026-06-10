using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Dto;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Util;

namespace Soteo.Core.Abilities;

public sealed class ProjectileBurstAbility : Ability
{
    public override CanTarget Targeting => CanTarget.Nothing;
    public override Scalable<double> StaticRange => 1000;
    public override Scalable<double> StaticCooldown => 5;

    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        var entityManager = context.GetRequiredService<IEntityManager>();
        for (double azimuth = 0; azimuth < 360; azimuth += 1)
        {
            Vector2 target = context.User.Position + Maths.AzimuthToDirection(azimuth) * 1000;
            double speed = 150 + 15 * Math.Sin(Maths.Deg2Rad(azimuth) * 20);
            entityManager.SpawnProjectile(context, speed, target);
        }
    }
}
