using System.Diagnostics.CodeAnalysis;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;
using static Soteo.Gameplay.Nodes.Entities.Entity;

namespace Soteo.Gameplay.Nodes.Entities;

public abstract class Projectile : IEntity
{
    protected sealed class ProjectileNode(Projectile projectile) : Area2D
    {
        public override void _PhysicsProcess(float delta)
        {
            if (IsServer) projectile._PhysicsProcessServer(delta);
        }
    }
    
    private readonly ClientDependency<ICamera> _camera;
    
    private readonly EntityProperties _properties;
    
    protected Projectile
    (
        Guid id,
        Unit source,
        Ability ability,
        float speed,
        PackedScene scene,
        ClientDependency<ICamera> camera,
        IShard shard
    )
    {
        Id = id;
        Source = source;
        Ability = ability;
        Speed = speed;
        _camera = camera;
        
        Node = new ProjectileNode(this) { Name = $"{GetType().Name} {id}" };
        scene.InstanceAndReparentTo(Node);
        _properties = Node.GetNode<EntityProperties>("Properties");
        shard.EntityRoot.AddChild(Node);
    }
    
    public event Action Removed = delegate {};
    
    public Guid Id { get; }
    [MemberNotNullWhen(false, nameof(Node))] public bool IsRemoved { get; private set; }
    public Vector2 Position { get; set; }
    public float Azimuth { get; set; }
    protected Unit? Source { get => field.AsValid(); set; }
    protected Ability Ability { get; private set; }
    protected float Speed { get; set; }
    protected ProjectileNode? Node => field.AsValid();
    
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
        if (IsRemoved) return;
        var s = (ProjectileSnapshot)snapshot;
        if (s.Position != null)
        {
            Position = s.Position.Value;
            Node.Position = RoundVisualPositionToPixelPerfect(s.Position.Value, _camera.Value,
                _properties.HalfPixelXVisualOffset, _properties.HalfPixelYVisualOffset);
        }
        if (s.Azimuth != null) Azimuth = s.Azimuth.Value;
        if (s.Ability != null) Ability = s.Ability;
        if (s.Speed != null) Speed = s.Speed.Value;
    }
    
    public void Remove()
    {
        if (IsRemoved) return;
        IsRemoved = true;
        Node.QueueFree();
        Removed();
    }
    
    [MemberNotNull(nameof(Node))]
    public virtual void _PhysicsProcessServer(float delta)
    {
        Node.Required.Position = Position;
    }
}