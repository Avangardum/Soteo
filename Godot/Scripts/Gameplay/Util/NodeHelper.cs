using Soteo.Gameplay.Interfaces;
using Soteo.Shared;

namespace Soteo.Gameplay.Util;

public static class NodeHelper
{
    /// <summary>
    /// Round a visual position value to a value that will allow pixel perfect rendering, without artifacts due to
    /// the sprite's pixels having noninteger position, therefore rendering between screen pixels
    /// </summary>
    /// <param name="halfPixelXOffset">
    /// Whether a half screen pixel x offset should be applied. Used when a sprite's x position ends in .5 or
    /// for the camera when the viewport width is odd, since normal rounding would cause screen pixels to be between
    /// world pixels.
    /// </param>
    /// <param name="halfPixelYOffset">
    /// Whether a half screen pixel y offset should be applied. Used when a sprite's y position ends in .5 or
    /// for the camera when the viewport height is odd, since normal rounding would cause screen pixels to be between
    /// world pixels.
    /// </param>
    public static Vector2 RoundPositionToPixelPerfect
    (
        Vector2 value,
        ICamera? camera,
        bool isCamera,
        bool halfPixelXOffset,
        bool halfPixelYOffset
    )
    {
        if (camera == null) return value;
        
        // If zoom is even, an entity with the half-world-pixel offset would have this offset equal to an integer
        // number of screen pixels, so it's already pixel perfect, no adjustment is applied. For camera, however,
        // odd viewport size without zoom causes a half-pixel offset between world pixel grid and screen pixel grid,
        // which persists at any zoom, so it's compensated regardless of zoom.
        if (!isCamera && Maths.IsMultipleOf(2, camera.TrueZoom))
            halfPixelXOffset = halfPixelYOffset = false;
        
        double screenPixelSizeInWorldPixels = 1 / camera.TrueZoom;
        double roundedX = halfPixelXOffset ? Maths.RoundToMultipleOfPlusHalf(screenPixelSizeInWorldPixels, value.x) :
            Maths.RoundToMultipleOf(screenPixelSizeInWorldPixels, value.x);
        double roundedY = halfPixelYOffset ? Maths.RoundToMultipleOfPlusHalf(screenPixelSizeInWorldPixels, value.y) :
            Maths.RoundToMultipleOf(screenPixelSizeInWorldPixels, value.y);
        return Vector2.New(roundedX, roundedY);
    }
}
