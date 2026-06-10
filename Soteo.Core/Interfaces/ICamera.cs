using System.Numerics;

namespace Soteo.Core.Interfaces;

public interface ICamera
{
    event Action ZoomChanged;
    Vector2 Position { get; }
    double Zoom { get; }
}
