namespace Soteo.Gameplay.Interfaces;

public interface IEntityNode
{
    IEntity? Entity { get; set; }
    GdVector2 Position { get; set; }
}