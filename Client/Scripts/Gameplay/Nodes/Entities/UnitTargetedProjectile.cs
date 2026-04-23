using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

public abstract class UnitTargetedProjectile : Projectile
{
    private bool _didHit;
    
    protected Unit? Target { get => field.AsValid(); set; }

    protected UnitTargetedProjectile
    (
        Guid id,
        Unit source,
        Ability ability,
        float speed,
        ClientDependency<ICamera> camera,
        Unit target
    ) : base(id, source, ability, speed, camera)
    {
        Target = target;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (!IsServer) return;
        
        if (_didHit || Target == null)
        {
            QueueFree();
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
            // Update position for the last time and defer freeing the node to the next frame so that clients receive a
            // snapshot where the projectile reaches the target, to prevent it from visually disappearing near the
            // target
            Position = Target.Position;
            _didHit = true;
        }
    }
    
    protected abstract void Hit();
}