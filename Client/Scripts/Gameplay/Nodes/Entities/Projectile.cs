using System.Diagnostics.CodeAnalysis;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

public abstract class Projectile : Entity<Projectile.ProjectileNode>
{
    public sealed class ProjectileNode(Projectile projectile) : Area2D
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
    ) : base(id, camera)
    {
        Source = source;
        Ability = ability;
        Speed = speed;
        _camera = camera;
        
        Node = new ProjectileNode(this) { Name = $"{GetType().Name} {id}" };
        scene.InstanceAndReparentTo(Node);
        _properties = Node.GetNode<EntityProperties>("Properties");
        shard.EntityRoot.AddChild(Node);
    }

    protected override ProjectileNode Node { get; }

    public override Vector2 Position
    {
        get;
        set
        {
            field = value;
            Node.Position = RoundVisualPositionToPixelPerfect(value, _camera.Value,
                _properties.HalfPixelXVisualOffset, _properties.HalfPixelYVisualOffset);
        }
    }
    
    protected Unit Source { get; set; }
    protected Ability Ability { get; private set; }
    protected float Speed { get; set; }
    
    public override EntitySnapshot CreateSnapshot()
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

    public override void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        if (IsRemoved) return;
        var s = (ProjectileSnapshot)snapshot;
        if (s.Position != null) Position = s.Position.Value;
        if (s.Azimuth != null) Azimuth = s.Azimuth.Value;
        if (s.Ability != null) Ability = s.Ability;
        if (s.Speed != null) Speed = s.Speed.Value;
    }
    
    [MemberNotNull(nameof(Node))]
    public virtual void _PhysicsProcessServer(float delta)
    {
        Node.Required.Position = Position;
    }

    protected override void OnZoomChanged()
    {
        
    }
}