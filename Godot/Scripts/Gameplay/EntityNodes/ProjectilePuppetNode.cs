using Soteo.Core.Entities;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;

namespace Soteo.Main.Gameplay.EntityNodes;

public sealed class ProjectilePuppetNode : Node2D, IProjectilePuppetNode
{
    // If the sprite has pixel position with .5 as fractional part in any dimension
    // (used to center sprites with odd sizes), the following fields help compensate
    // it for pixel perfect rendering. See NodeHelper for details.
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

    public Vector2 PositionMeters
    {
        get => Position.ToSys() / Const.PixelsInMeter;
        set => Position = value.ToGd() * Const.PixelsInMeter;
    }
}
