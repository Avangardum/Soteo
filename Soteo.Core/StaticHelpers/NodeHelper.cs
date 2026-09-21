using System.Numerics;
using Soteo.Util;

namespace Soteo.Core.StaticHelpers;

public static class NodeHelper
{
    /// <summary>
    /// Round a visual position value to a value that allows pixel perfect rendering without artifacts due to
    /// sprite pixels having fractional screen pixel position, therefore rendering between screen pixels.
    /// </summary>
    /// <param name="halfPixelXOffset">
    /// Whether a half screen pixel x offset should be applied. Used when a sprite x pixel position ends in .5 or
    /// for the camera when the viewport width is odd, since normal rounding would cause
    /// fractional screen pixel position.
    /// </param>
    /// <param name="halfPixelYOffset">
    /// Whether a half screen pixel y offset should be applied. Used when a sprite's y position ends in .5 or
    /// for the camera when the viewport height is odd, since normal rounding would cause
    /// fractional screen pixel position.
    /// </param>
    public static Vector2 RoundPositionToPixelPerfect
    (
        Vector2 value,
        double zoom,
        bool isCamera,
        bool halfPixelXOffset,
        bool halfPixelYOffset
    )
    {
        // If zoom is even, a sprite with a half-world-pixel offset would have this offset equal to an integer
        // number of screen pixels, so it's already pixel perfect, no adjustment is applied. For camera, however,
        // odd viewport size without zoom causes a half-pixel offset between world pixel grid and screen pixel grid,
        // which persists at any zoom, so it's compensated regardless of zoom.
        if (!isCamera && Maths.IsMultipleOf(2, zoom))
            halfPixelXOffset = halfPixelYOffset = false;

        Vector2 valueInWorldPixels = value * Const.PixelsInMeter;
        double screenPixelSizeInWorldPixels = 1 / zoom;
        double roundedXInWorldPixels = halfPixelXOffset ?
            Maths.RoundToMultipleOfPlusHalf(screenPixelSizeInWorldPixels, valueInWorldPixels.X) :
            Maths.RoundToMultipleOf(screenPixelSizeInWorldPixels, valueInWorldPixels.X);
        double roundedYPx = halfPixelYOffset ?
            Maths.RoundToMultipleOfPlusHalf(screenPixelSizeInWorldPixels, valueInWorldPixels.Y) :
            Maths.RoundToMultipleOf(screenPixelSizeInWorldPixels, valueInWorldPixels.Y);
        return Vector2.New(roundedXInWorldPixels, roundedYPx) / Const.PixelsInMeter;
    }
}
