using System.Numerics;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Core.Gameplay.Entities;

public sealed class Projectile : Entity<IProjectileNode>
{
    private readonly IServiceProvider _serviceProvider;
    
    private AbilityContext _abilityContext;
    private double _speed;
    private bool _didHit;
    
    public Projectile
    (
        Guid id,
        AbilityContext abilityContext,
        double speed,
        IProjectileNode node,
        IServiceProvider serviceProvider
    ) : base(id, node)
    {
        _abilityContext = abilityContext;
        _speed = speed;
        _serviceProvider = serviceProvider;
    }

    public override Vector2 Position
    {
        get => base.Position;
        set
        {
            base.Position = value;
            Node?.Position = value;
        }
    }
    
    public override EntitySnapshot CreateSnapshot()
    {
        return new ProjectileSnapshot
        {
            Id = Id,
            Position = Position,
            Azimuth = Azimuth,
            AbilityContext = _abilityContext.Deflate(),
            Speed = _speed
        };
    }

    public override void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        base.ReplicateSnapshot(snapshot);
        var s = (ProjectileSnapshot)snapshot;
        _abilityContext = s.AbilityContext.Inflate(_serviceProvider);
        _speed = s.Speed;
    }

    public void Tick(double delta)
    {
        if (_didHit)
        {
            Remove();
            return;
        }
        
        Vector2 targetPosition =
            _abilityContext.TargetUnit?.Position ?? _abilityContext.TargetPosition ?? Position;
        Vector2 directionToTarget = targetPosition - Position;
        double movementLength = _speed * delta;
        if (movementLength * movementLength < directionToTarget.LengthSquared())
        {
            Position += Vector2.Normalize(directionToTarget) * movementLength;
        }
        else
        {
            _didHit = true;
            _abilityContext.Ability.OnProjectileHit(_abilityContext);
            // Update position for the last time and defer removing the entity to the next frame so that clients receive
            // a snapshot where the projectile reaches the target, to prevent it from visually disappearing near the
            // target. If targeting a unit, offset it 1 pixel up so that when coming from above it doesn't flicker for 1
            // frame in front of the target.
            Position = _abilityContext.TargetUnit != null ?
                _abilityContext.TargetUnit.Position - Vector2.UnitY :
                targetPosition;
        }
    }
}
