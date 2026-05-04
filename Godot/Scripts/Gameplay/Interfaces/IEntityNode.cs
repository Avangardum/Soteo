namespace Soteo.Gameplay.Interfaces;

public interface IEntityNode
{
    Node2D Node { get; }
    IEntity? Entity { get; set; }
}