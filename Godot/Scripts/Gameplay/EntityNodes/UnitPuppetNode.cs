using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.EntityNodes;

public sealed class UnitPuppetNode : Node2D, IEntityNode
{
    public Node2D Node => this;
    
    public UnitPuppet? UnitPuppet { get; set; }
    
    public IEntity? Entity
    {
        get => UnitPuppet;
        set => UnitPuppet = (UnitPuppet?)value;
    }
    
    public AnimatedSprite Sprite { get; private set; } = null!;
    public AzimuthIndicator AzimuthIndicator { get; private set; } = null!;
    public EntityProperties Properties { get; private set; } = null!;
    
    public override void _Ready()
    {
        Sprite = GetNode<AnimatedSprite>("Visuals/AnimatedSprite");
        AzimuthIndicator = GetNode<AzimuthIndicator>("Visuals/AzimuthIndicator");
        Properties = GetNode<EntityProperties>("Properties");
        
        Sprite.Playing = true;
    }
}