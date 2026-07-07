using System.Numerics;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Entities;

public sealed class Projectile : Entity<IProjectileNode>
{
    private readonly IServiceProvider _serviceProvider;
    
    private AbilityContext _abilityContext;
    private double _speed;
    private ProjectileTarget _target;
    private bool _didHit;
    
    /// <summary>
    /// If true, the projectile is in invalid state due to some non-nullable fields having null value, which are
    /// expected to be set via snapshot replication immediately
    /// </summary>
    private bool _isSnapshotReplicationPendingToFinishInit;
    
    public Projectile
    (
        Guid id,
        AbilityContext abilityContext,
        double speed,
        ProjectileTarget target,
        IProjectileNode node,
        IServiceProvider serviceProvider
    ) : base(id, node)
    {
        _abilityContext = abilityContext;
        _speed = speed;
        _target = target;
        _serviceProvider = serviceProvider;
    }
    
    public static Projectile FromSnapshot
    (
        ProjectileSnapshot snapshot,
        IProjectileNode node,
        IServiceProvider serviceProvider
    )
    {
        // Null is passed to parameters that require objects potentially referencing other entities.
        // This is because during snapshot replication these other entities may be not instantiated yet.
        // These values are set via snapshot replication after all entities are instantiated.
        return new Projectile
        (
            snapshot.Id,
            abilityContext: null!,
            snapshot.Speed,
            target: null!,
            node,
            serviceProvider
        )
        {
            _isSnapshotReplicationPendingToFinishInit = true
        };
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
            IsRemoved = IsRemoved,
            Position = Position,
            Azimuth = Azimuth,
            AbilityContext = _abilityContext.ToSnapshot(),
            Speed = _speed,
            Target = _target.ToSnapshot(),
        };
    }

    public override void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        base.ReplicateSnapshot(snapshot);
        var s = (ProjectileSnapshot)snapshot;
        _abilityContext = AbilityContext.FromSnapshot(s.AbilityContext, _serviceProvider);
        _speed = s.Speed;
        _target = ProjectileTarget.FromSnapshot(s.Target, _serviceProvider);
        _isSnapshotReplicationPendingToFinishInit = false;
    }

    public void Tick(double delta)
    {
        if (_isSnapshotReplicationPendingToFinishInit)
            throw new InvalidOperationException("Snapshot replication is pending to finish initialization");
        
        if (_didHit)
        {
            Remove();
            return;
        }
        
        Vector2 targetPosition = _target.IsUnit ? _target.Unit.Position : _target.Position.Value;
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
            Position = _target.IsUnit ? targetPosition - Vector2.UnitY : targetPosition;
        }
    }
}
