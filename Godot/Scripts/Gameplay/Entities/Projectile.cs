using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.EntityNodes;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;
using Soteo.Shared.Extensions;

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
        get;
        set
        {
            field = value;
            if (IsRemoved) return;
            if (IsServer)
            {
                Node.Position = value;
            }
            else
            {
                Node.Position = RoundVisualPositionToPixelPerfect
                (
                    value,
                    Node.Properties.HalfPixelXVisualOffset,
                    Node.Properties.HalfPixelYVisualOffset
                );
            }
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
        var s = (ProjectileSnapshot)snapshot;
        Position = s.Position;
        Azimuth = s.Azimuth;
        AbilityContext = s.AbilityContext.Inflate(_serviceProvider);
        Speed = s.Speed;
    }
    
    public virtual void _PhysicsProcessServer(ProjectileNode node, double delta) { }

    protected override void OnZoomChanged()
    {
        // todo
    }
}