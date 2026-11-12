namespace Soteo.Core.Entities;

public interface IEntityNodeManager
{
    T AddNode<T>(Guid id) where T : class, IEntityNode;
    void RemoveNode(Guid id);
}
