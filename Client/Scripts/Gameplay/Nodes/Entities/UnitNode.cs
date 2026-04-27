using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

public sealed class UnitNode : KinematicBody2D
{
    public UnitNode(Unit unit, PackedScene scene, IShard shard)
    {
        scene.InstanceAndReparentTo(this);
        shard.EntityRoot.AddChild(this);
            
        Unit = unit;
        Visuals = GetNode<Node2D>("Visuals");
        Sprite = GetNode<AnimatedSprite>("Visuals/AnimatedSprite");
        AzimuthLine = GetNode<Line2D>("Visuals/AzimuthLine");
        Properties = GetNode<EntityProperties>("Properties");
    }
        
    public Unit Unit { get; }
    public Node2D Visuals { get; }
    public AnimatedSprite Sprite { get; }
    public Line2D AzimuthLine { get; }
    public EntityProperties Properties { get; }
        
    public override void _PhysicsProcess(float delta)
    {
        if (IsServer) Unit._PhysicsProcessServer(this, delta);
        else Unit._PhysicsProcessClient(this, delta);
    }
}