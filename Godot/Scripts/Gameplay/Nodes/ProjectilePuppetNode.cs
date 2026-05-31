using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Gameplay.Nodes;

public sealed class ProjectilePuppetNode : Node2D, IProjectilePuppetNode
{
    // If the sprite has position with .5 as fractional part in any dimension (used to center sprites with odd sizes),
    // the following fields help compensate it for pixel perfect rendering. See NodeHelper for details.
    [Export] private bool _halfPixelXVisualOffset;
    [Export] private bool _halfPixelYVisualOffset;
    
    public bool HalfPixelXVisualOffset => _halfPixelXVisualOffset;
    public bool HalfPixelYVisualOffset => _halfPixelYVisualOffset;
    
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
}
