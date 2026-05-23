using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Services;

public sealed class EntityNodeManager(IShardNode shard, IEntityNodePool pool) : IEntityNodeManager
{
    private readonly Dictionary<Guid, IEntityNode> _nodes = [];
    
    public T AddNode<T>(Guid id) where T : class, IEntityNode
    {
        T node = pool.GetNode<T>();
        ((Node)(object)node).Name = $"{typeof(T).Name} {id}";
        shard.EntityRoot.AddChild((Node)(object)node);
        _nodes[id] = node;
        return node;
    }
    
    public void RemoveNode(Guid id)
    {
        IEntityNode node = _nodes[id];
        _nodes.Remove(id);
        
        if (node is IDeferredRemovalEntityNode deferred)
        {
            deferred.WaitUntilCanRemoveAsync()
                .ContinueWithinContext(() => RemoveNodeImmediately(node))
                .CollectException();
        }
        else
        {
            RemoveNodeImmediately(node);
        }
    }
    
    private void RemoveNodeImmediately(IEntityNode node)
    {
        shard.EntityRoot.RemoveChild((Node)node);
        pool.ReturnNode(node);
    }
}
