using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.EntityNodes;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;

namespace Soteo.Gameplay.Entities;

public abstract class Projectile : Entity<ProjectileNode>
{
    private readonly IServiceProvider _serviceProvider;
    
    protected Projectile
    (
        Guid id,
        AbilityContext abilityContext,
        double speed,
        ProjectileNode node,
        IServiceProvider serviceProvider
    ) : base(id, node, serviceProvider.GetRequiredService<ClientDependency<ICamera>>())
    {
        AbilityContext = abilityContext;
        Speed = speed;
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
    
    protected AbilityContext AbilityContext { get; private set; }
    protected double Speed { get; private set; }
    
    public override EntitySnapshot CreateSnapshot()
    {
        return new ProjectileSnapshot
        {
            Id = Id,
            Position = Position,
            Azimuth = Azimuth,
            AbilityContext = AbilityContext.Deflate(),
            Speed = Speed
        };
    }

    public override void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        base.ReplicateSnapshot(snapshot);
        var s = (ProjectileSnapshot)snapshot;
        AbilityContext = s.AbilityContext.Inflate(_serviceProvider);
        Speed = s.Speed;
    }

    public virtual void PhysicsProcess(ProjectileNode node, double delta) { }
}