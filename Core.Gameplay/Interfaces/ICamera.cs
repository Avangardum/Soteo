using System.Numerics;

namespace Soteo.Core.Gameplay.Interfaces;

public interface ICamera
{
    event Action ZoomChanged;
    
    Vector2 Position { get; }
    
    /// <summary>
    /// True zoom is a numeric value representing coefficient of enlargement of zoomed in objects.
    /// When camera zoom is mentioned anywhere in the project, it refers to this value, not the Godot camera's value.
    /// </summary>
    double TrueZoom { get; } // todo rename
}
