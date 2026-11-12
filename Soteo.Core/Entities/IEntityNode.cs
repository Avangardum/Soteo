using System.Numerics;

namespace Soteo.Core.Interfaces;

public interface IEntityNode
{
    IEntity? Entity { get; set; }
    Vector2 PositionMeters { get; set; }
}
