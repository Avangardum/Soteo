namespace Soteo.Gameplay.Nodes.Entities;

public sealed class UnitProperties : Node
{
    // If the sprite has position with .5 as fractional part in any dimension (used to center sprites with odd sizes),
    // the following fields help compensate it by ensuring that the Visuals node's global position also has .5 fraction
    // in matching dimensions, so that global position of the sprite ends up whole, which is necessary to achieve
    // pixel perfection and avoid artifacts.
    [Export] public bool HalfPixelXVisualOffset;
    [Export] public bool HalfPixelYVisualOffset;
}