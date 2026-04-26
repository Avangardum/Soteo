using System.Diagnostics.CodeAnalysis;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

public abstract class Projectile : Entity<Projectile.ProjectileNode>
{
    public sealed class ProjectileNode : Area2D
    {
        private readonly Projectile _projectile;
        public EntityProperties Properties { get; }

        public ProjectileNode(Projectile projectile, PackedScene scene, IShard shard)
        {
            _projectile = projectile;
            scene.InstanceAndReparentTo(this);
            shard.EntityRoot.AddChild(this);
            Properties = GetNode<EntityProperties>("Properties");
        }

        public override void _PhysicsProcess(float delta)
        {
            if (IsServer) _projectile._PhysicsProcessServer(this, delta);
        }
    }
    
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
        
        Node = new ProjectileNode(this, scene, shard) { Name = $"{GetType().Name} {id}" };
    }

    [MemberNotNullWhen(false, nameof(Node))] public override bool IsRemoved { get; protected set; }
    protected override ProjectileNode? Node => field.AsValid();

    public override Vector2 Position
    {
        get;
        set
        {
            field = value;
            if (IsRemoved) return;
            Node.Position = RoundVisualPositionToPixelPerfect(value, Node.Properties.HalfPixelXVisualOffset,
                Node.Properties.HalfPixelYVisualOffset);
        }
    }
    
    protected Unit Source { get; private set; }
    protected Ability Ability { get; private set; }
    protected float Speed { get; private set; }
    
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
        var s = (ProjectileSnapshot)snapshot;
        if (s.Position != null) Position = s.Position.Value;
        if (s.Azimuth != null) Azimuth = s.Azimuth.Value;
        if (s.Ability != null) Ability = s.Ability;
        if (s.Speed != null) Speed = s.Speed.Value;
    }
    
    public virtual void _PhysicsProcessServer(ProjectileNode node, float delta) { }

    protected override void OnZoomChanged()
    {
        
    }
}