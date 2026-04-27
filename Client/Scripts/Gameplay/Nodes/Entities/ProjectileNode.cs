using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

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