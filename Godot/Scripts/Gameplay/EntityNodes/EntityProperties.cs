namespace Soteo.Gameplay.EntityNodes;

public sealed class EntityProperties : Node
{
    // If the sprite has position with .5 as fractional part in any dimension (used to center sprites with odd sizes),
    // the following fields help compensate it for pixel perfect rendering. See NodeHelper for details.
    [Export] public bool HalfPixelXVisualOffset;
    [Export] public bool HalfPixelYVisualOffset;
}
