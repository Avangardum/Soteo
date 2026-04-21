namespace Soteo.Gameplay.Interfaces;

public interface ICamera
{
    Vector2 Position { get; }
    
    /// <summary>
    /// True zoom is a numeric value representing coefficient of enlargement of zoomed in objects.
    /// When camera zoom is mentioned anywhere in the project, it refers to this value, not the Godot camera's value.
    /// </summary>
    float TrueZoom { get; }
}