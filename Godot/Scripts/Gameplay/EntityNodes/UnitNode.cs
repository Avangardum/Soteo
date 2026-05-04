using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.EntityNodes;

public sealed class UnitNode : KinematicBody2D, IEntityNode
{
    public Node2D Node => this;
        
    public Unit? Unit { get; set; }
    
    public IEntity? Entity
    {
        get => Unit;
        set => Unit = (Unit?)value;
    }
    
    public Node2D Visuals { get; private set; } = null!;
    public AnimatedSprite Sprite { get; private set; } = null!;
    public AzimuthIndicator AzimuthIndicator { get; private set; } = null!;
    public EntityProperties Properties { get; private set; } = null!;
        
    public override void _Ready()
    {
        Visuals = GetNode<Node2D>("Visuals");
        Sprite = GetNode<AnimatedSprite>("Visuals/AnimatedSprite");
        AzimuthIndicator = GetNode<AzimuthIndicator>("Visuals/AzimuthIndicator");
        Properties = GetNode<EntityProperties>("Properties");
        
        Sprite.Playing = true;
    }
    
    public override void _PhysicsProcess(float delta)
    {
        if (IsServer) Unit?._PhysicsProcessServer(this, delta);
        else Unit?._PhysicsProcessClient(this, delta);
    }
}