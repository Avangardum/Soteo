namespace Soteo.Gameplay.Interfaces;

public interface IEntityNodePool
{
    T GetNode<T>() where T : class, IEntityNode;
    void ReturnNode(IEntityNode node);
}
