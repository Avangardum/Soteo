using Soteo.Gameplay.Entities;
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
    
    public bool HalfPixelXVisualOffset => _properties.HalfPixelXVisualOffset;
    public bool HalfPixelYVisualOffset => _properties.HalfPixelYVisualOffset;

    public override void _Ready()
    {
        _properties = GetNode<EntityProperties>("Properties");
    }
}