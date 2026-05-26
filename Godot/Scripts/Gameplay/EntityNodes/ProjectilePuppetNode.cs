using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.EntityNodes;

public sealed class ProjectilePuppetNode : Node2D, IProjectilePuppetNode
{
    private EntityProperties _properties = null!;
    
    public ProjectilePuppet? ProjectilePuppet { get; set; }
    
    public IEntity? Entity
    {
        get => ProjectilePuppet;
        set => ProjectilePuppet = (ProjectilePuppet?)value;
    }
    
    public new Vector2 Position
    {
        get => base.Position.ToSys();
        set => base.Position = value.ToGd();
    }
    
    public bool HalfPixelXVisualOffset => _properties.HalfPixelXVisualOffset;
    public bool HalfPixelYVisualOffset => _properties.HalfPixelYVisualOffset;

    public override void _Ready()
    {
        _properties = GetNode<EntityProperties>("Properties");
    }
}
