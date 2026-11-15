namespace Soteo.Core.Entities;

public interface IEntityNode
{
    Entity? Entity { get; set; }
    Vector2 PositionMeters { get; set; }
}
