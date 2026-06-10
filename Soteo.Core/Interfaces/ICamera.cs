using System.Numerics;

namespace Soteo.Core.Gameplay.Interfaces;

public interface ICamera
{
    event Action ZoomChanged;
    Vector2 Position { get; }
    double Zoom { get; }
}
