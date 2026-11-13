using Soteo.Core.Entities;

namespace Soteo.Main.Gameplay.EntityNodes;

public interface IEntityNodePool
{
    T GetNode<T>() where T : class, IEntityNode;
    void ReturnNode(IEntityNode node);
}
