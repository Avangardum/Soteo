using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;
using static Soteo.Gameplay.Nodes.Entities.Entity;

namespace Soteo.Gameplay.Nodes.Entities;

public abstract class Projectile : Area2D, IEntity
{
    private readonly ClientDependency<ICamera> _camera;
    
    private readonly EntityProperties _properties;
    
    protected Projectile
    (
        Guid id,
        Unit source,
        Ability ability,
        float speed,
        PackedScene scene,
        ClientDependency<ICamera> camera
    )
    {
        Name = id.ToString();
        
        Id = id;
        Source = source;
        Ability = ability;
        Speed = speed;
        _camera = camera;
        
        scene.InstanceAndReparentTo(this);
        _properties = GetNode<EntityProperties>("Properties");
    }
    
    public event Action Removed = delegate {};
    
    public Guid Id { get; }
    public float Azimuth { get; set; }
    protected Unit? Source { get => field.AsValid(); set; }
    protected Ability Ability { get; private set; }
    protected float Speed { get; set; }

    public Node2D Node => this;
    
    public EntitySnapshot CreateSnapshot()
    {
        return new ProjectileSnapshot(Id)
        {
            Position = Position,
            Azimuth = Azimuth,
            Ability = Ability,
            Speed = Speed
        };
        // todo source and target
    }

    public void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        var s = (ProjectileSnapshot)snapshot;
        if (s.Position != null) Position = RoundVisualPositionToPixelPerfect(s.Position.Value,
            _camera.Value, _properties.HalfPixelXVisualOffset, _properties.HalfPixelYVisualOffset);
        if (s.Azimuth != null) Azimuth = s.Azimuth.Value;
        if (s.Ability != null) Ability = s.Ability;
        if (s.Speed != null) Speed = s.Speed.Value;
    }
    
    public void Remove()
    {
        GetParent().RemoveChild(this);
        Removed();
    }
    
    [Obsolete(FreeErrorMessage, true)]
    public new void Free() => throw new InvalidOperationException(FreeErrorMessage);
    
    [Obsolete(FreeErrorMessage, true)]
    public new void QueueFree() => throw new InvalidOperationException(FreeErrorMessage);
}