using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.EntityNodes;

public sealed class ProjectileNode : Area2D, IEntityNode
{
    public Node2D Node => this;
    
    public Projectile? Projectile { get; set; }
    
    public IEntity? Entity
    {
        get => Projectile;
        set => Projectile = (Projectile?)value;
    }
    
    public EntityProperties Properties { get; private set; } = null!;

    public override void _Ready()
    {
        Properties = GetNode<EntityProperties>("Properties");
    }

    public override void _PhysicsProcess(float delta)
    {
        if (IsServer) Projectile?._PhysicsProcessServer(this, delta);
    }
}