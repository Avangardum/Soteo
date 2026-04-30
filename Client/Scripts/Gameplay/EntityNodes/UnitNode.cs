using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.EntityNodes;

public sealed class UnitNode : KinematicBody2D
{
    public UnitNode(Unit unit, PackedScene scene, IShard shard)
    {
        scene.InstanceAndReparentTo(this);
        shard.EntityRoot.AddChild(this);
            
        Unit = unit;
        Visuals = GetNode<Node2D>("Visuals");
        Sprite = GetNode<AnimatedSprite>("Visuals/AnimatedSprite");
        AzimuthIndicator = GetNode<AzimuthIndicator>("Visuals/AzimuthIndicator");
        Properties = GetNode<EntityProperties>("Properties");
        
        Sprite.Playing = true;
    }
        
    public Unit Unit { get; }
    public Node2D Visuals { get; }
    public AnimatedSprite Sprite { get; }
    public AzimuthIndicator AzimuthIndicator { get; }
    public EntityProperties Properties { get; }
        
    public override void _PhysicsProcess(float delta)
    {
        if (IsServer) Unit._PhysicsProcessServer(this, delta);
        else Unit._PhysicsProcessClient(this, delta);
    }
}