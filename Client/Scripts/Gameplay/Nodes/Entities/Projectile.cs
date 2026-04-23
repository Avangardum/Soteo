using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

public abstract class Projectile : Area2D, IEntity
{
    private readonly ClientDependency<ICamera> _camera;
    
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
        Id = id;
        Source = source;
        Ability = ability;
        Speed = speed;
        _camera = camera;
        
        scene.InstanceAndReparentTo(this);
        Name = id.ToString();
    }
    
    public Guid Id { get; }
    public float Azimuth { get; set; }
    protected Unit? Source { get => field.AsValid(); set; }
    protected Ability Ability { get; private set; }
    protected float Speed { get; set; }

    public Node2D Node => this;
    
    public EntitySnapshot CreateSnapshot()
    {
        return new EntitySnapshot
        {
            Id = Id,
            Position = Position,
            Azimuth = Azimuth,
            Ability = Ability,
            Speed = Speed,
        };
        // todo source and target
    }

    public void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        if (snapshot.Position != null) Position = snapshot.Position.Value;
        if (snapshot.Azimuth != null) Azimuth = snapshot.Azimuth.Value;
        if (snapshot.Ability != null) Ability = snapshot.Ability;
        if (snapshot.Speed != null) Speed = snapshot.Speed.Value;
    }
    
    // todo pixel perfect rendering
}