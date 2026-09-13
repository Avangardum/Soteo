using System.Numerics;

namespace Soteo.Core.Interfaces;

public interface ICamera
{
    event Action ZoomChanged;
    Vector2 PositionM { get; }
    double Zoom { get; }
}
