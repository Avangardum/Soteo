namespace Soteo.Gameplay.Interfaces;

public interface IEntityNode
{
    IEntity? Entity { get; set; }
    Vector2 Position { get; set; }
}