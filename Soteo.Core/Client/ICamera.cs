using System.Numerics;

namespace Soteo.Core.Client;

public interface ICamera
{
    event Action ZoomChanged;
    Vector2 PositionM { get; }
    double Zoom { get; }
}
