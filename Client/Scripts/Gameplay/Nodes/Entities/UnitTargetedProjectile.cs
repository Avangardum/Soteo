using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Nodes.Entities;

public abstract class UnitTargetedProjectile : Projectile
{
    private bool _didHit;
    
    protected Unit Target { get; private set; }

    protected UnitTargetedProjectile
    (
        Guid id,
        Unit source,
        Ability ability,
        float speed,
        Unit target,
        PackedScene scene,
        ClientDependency<ICamera> camera,
        IShard shard
    ) : base(id, source, ability, speed, scene, camera, shard)
    {
        Target = target;
    }

    public override void _PhysicsProcessServer(ProjectileNode node, float delta)
    {
        base._PhysicsProcessServer(node, delta);
        
        if (_didHit)
        {
            Remove();
            return;
        }
        
        Vector2 directionToTarget = Target.Position - Position;
        float movementLength = Speed * delta;
        if (movementLength * movementLength < directionToTarget.LengthSquared())
        {
            Position += directionToTarget.Normalized() * movementLength;
        }
        else
        {
            Hit();
            // Update position for the last time and defer removing the entity to the next frame so that clients receive
            // a snapshot where the projectile reaches the target, to prevent it from visually disappearing near the
            // target. Offset it 1 pixel up so that when coming from above it doesn't flicker for 1 frame in front of
            // the target.
            Position = Target.Position + Vector2.Up;
            _didHit = true;
        }
    }
    
    protected abstract void Hit();
}