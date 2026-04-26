using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

public abstract class UnitTargetedProjectile : Projectile
{
    private bool _didHit;
    
    protected UnitTargetedProjectile
    (
        Guid id,
        AbilityContext abilityContext,
        float speed,
        PackedScene scene,
        IServiceProvider serviceProvider
    ) : base(id, abilityContext, speed, scene, serviceProvider) { }

    public override void _PhysicsProcessServer(ProjectileNode node, float delta)
    {
        base._PhysicsProcessServer(node, delta);
        
        if (_didHit)
        {
            Remove();
            return;
        }
        
        Vector2 directionToTarget = AbilityContext.TargetUnit.Required.Position - Position;
        float movementLength = Speed * delta;
        if (movementLength * movementLength < directionToTarget.LengthSquared())
        {
            Position += directionToTarget.Normalized() * movementLength;
        }
        else
        {
            _didHit = true;
            AbilityContext.Ability.OnProjectileHit(AbilityContext);
            // Update position for the last time and defer removing the entity to the next frame so that clients receive
            // a snapshot where the projectile reaches the target, to prevent it from visually disappearing near the
            // target. Offset it 1 pixel up so that when coming from above it doesn't flicker for 1 frame in front of
            // the target.
            Position = AbilityContext.TargetUnit.Position + Vector2.Up;
        }
    }
}