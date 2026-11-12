namespace Soteo.Core.Entities.Projectiles;

public interface IProjectilePuppetNode : IEntityNode
{
    bool HalfPixelXVisualOffset { get; }
    bool HalfPixelYVisualOffset { get; }
}
