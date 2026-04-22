using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

public abstract class UnitTargetedProjectile : Projectile
{
    protected Unit? Target { get => field.AsValid(); set; }

    protected UnitTargetedProjectile(Guid id, Unit source, Ability ability, ICamera? camera, Unit target, float speed) :
        base(id, source, ability, camera)
    {
        Target = target;
        Speed = speed; // todo initialize Speed in Projectile
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        if (!IsServer) return;
        
        if (Target == null)
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
            // todo defer free
            QueueFree();
        }
    }
    
    protected abstract void Hit();
}