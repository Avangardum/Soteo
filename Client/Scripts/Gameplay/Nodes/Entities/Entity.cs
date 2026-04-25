using Soteo.Gameplay.Interfaces;
using Soteo.Shared;

namespace Soteo.Gameplay.Nodes.Entities;

public static class Entity
{
    /// <summary>
    /// Round a visual position value to a value that will allow pixel perfect rendering without artifacts due to
    /// the sprite's pixels having noninteger position, therefore rendering between screen pixels
    /// </summary>
    /// <param name="value">Value to round</param>
    /// <param name="camera">Camera</param>
    /// <param name="halfPixelXOffset">
    /// Whether a half screen pixel x offset should be applied. Use when the sprite's x position ends in .5
    /// </param>
    /// <param name="halfPixelYOffset">
    /// Whether a half screen pixel y offset should be applied. Use when the sprite's y position ends in .5
    /// </param>
    public static Vector2 RoundVisualPositionToPixelPerfect
    (
        Vector2 value,
        ICamera? camera,
        bool halfPixelXOffset,
        bool halfPixelYOffset
    )
    {
        // If zoom is not an integer, pixel perfect rendering is impossible
        if (camera == null || camera.TrueZoom % 1 != 0) return value;
        
        // If zoom is even, a world pixel with half pixel offset will be rendered as even number of screen pixels,
        // which will distribute equally in all directions, so pixel perfect rendering is possible without having
        // to compensate for this offset.
        if (camera.TrueZoom % 2 == 0) halfPixelXOffset = halfPixelYOffset = false;
        
        float screenPixelSizeInWorldPixels = 1 / camera.TrueZoom;
        float roundedX = halfPixelXOffset ? SoteoMath.RoundToMultipleOfPlusHalf(screenPixelSizeInWorldPixels, value.x) :
            SoteoMath.RoundToMultipleOf(screenPixelSizeInWorldPixels, value.x);
        float roundedY = halfPixelYOffset ? SoteoMath.RoundToMultipleOfPlusHalf(screenPixelSizeInWorldPixels, value.y) :
            SoteoMath.RoundToMultipleOf(screenPixelSizeInWorldPixels, value.y);
        return new Vector2(roundedX, roundedY);
    }
}