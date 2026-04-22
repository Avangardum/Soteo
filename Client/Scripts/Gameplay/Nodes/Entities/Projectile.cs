using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

public abstract class Projectile : Area2D, IEntity
{
    private readonly ICamera? _camera;
    
    public Guid Id { get; set; } // todo get only
    public float Azimuth { get; set; }
    protected Unit? Source { get => field.AsValid(); set; }
    protected Ability Ability { get; private set; }
    protected float Speed { get; set; }

    public Node2D Node => this; // todo null if invalid
    
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
    
    protected Projectile(Guid id, Unit source, Ability ability, ICamera? camera) // todo one sided dependency wrapper
    {
        Id = id;
        Source = source;
        Ability = ability;
        _camera = camera;
    }
    
    // todo pixel perfect rendering
}