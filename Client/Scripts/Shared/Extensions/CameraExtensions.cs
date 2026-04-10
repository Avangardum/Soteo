namespace Soteo.Shared.Extensions;

public static class CameraExtensions
{
    extension (Camera2D self)
    {
        /// <summary>
        /// This property works like Zoom in Godot 4: increasing it zooms in, decreasing zooms out
        /// </summary>
        public Vector2 TrueZoom
        {
            get => Vector2.One / self.Zoom;
            set => self.Zoom = Vector2.One / value;
        }
    }
}