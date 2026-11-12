namespace Soteo.Core.Entities;

public interface IEntityNode
{
    IEntity? Entity { get; set; }
    Vector2 PositionMeters { get; set; }
}
