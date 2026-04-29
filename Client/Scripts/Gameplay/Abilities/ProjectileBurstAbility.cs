using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;
using Soteo.Shared;

namespace Soteo.Gameplay.Abilities;

public sealed class ProjectileBurstAbility : Ability
{
    public override CanTarget Targeting => CanTarget.Nothing;
    public override Scalable<float> StaticRange => 1000;
    public override Scalable<float> StaticCooldown => 5;

    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        var entityManager = context.GetRequiredService<IEntityManager>();
        for (float azimuth = 0; azimuth <= 360; azimuth++)
        {
            Vector2 target = context.User.Position + SoteoMath.AzimuthToDirection(azimuth) * 1000;
            float speed = 150 + 15 * Mathf.Sin(Mathf.Deg2Rad(azimuth) * 20);
            entityManager.SpawnAttackProjectile(context with { TargetPosition = target }, speed);
        }
    }
}