using System.Numerics;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Core.Gameplay.Entities;

public sealed class TargetedProjectile : Projectile // todo collapse into Projectile, refactor targeting
{
    private bool _didHit;
    
    public TargetedProjectile
    (
        Guid id,
        AbilityContext abilityContext,
        double speed,
        IProjectileNode node,
        IServiceProvider serviceProvider
    ) : base(id, abilityContext, speed, node, serviceProvider) { }

    public override void Tick(double delta)
    {
        base.Tick(delta);
        
        if (_didHit)
        {
            Remove();
            return;
        }
        
        Vector2 targetPosition =
            AbilityContext.TargetUnit?.Position ?? AbilityContext.TargetPosition ?? Position;
        Vector2 directionToTarget = targetPosition - Position;
        double movementLength = Speed * delta;
        if (movementLength * movementLength < directionToTarget.LengthSquared())
        {
            Position += Vector2.Normalize(directionToTarget) * movementLength;
        }
        else
        {
            _didHit = true;
            AbilityContext.Ability.OnProjectileHit(AbilityContext);
            // Update position for the last time and defer removing the entity to the next frame so that clients receive
            // a snapshot where the projectile reaches the target, to prevent it from visually disappearing near the
            // target. If targeting a unit, offset it 1 pixel up so that when coming from above it doesn't flicker for 1
            // frame in front of the target.
            Position = AbilityContext.TargetUnit != null ?
                AbilityContext.TargetUnit.Position - Vector2.UnitY :
                targetPosition;
        }
    }
}
