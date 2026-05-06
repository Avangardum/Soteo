namespace Soteo.Gameplay.Interfaces;

public interface IEntityNodePool
{
    T GetNode<T>() where T : Node2D, IEntityNode;
    void ReturnNode(IEntityNode node);
}