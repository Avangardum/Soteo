namespace Soteo.Core.Gameplay.Interfaces;

public interface IProjectilePuppetNode : IEntityNode
{
    bool HalfPixelXVisualOffset { get; }
    bool HalfPixelYVisualOffset { get; }
}