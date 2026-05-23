using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Services;

public sealed class EntityNodeManager(IShard shard, IEntityNodePool pool) : IEntityNodeManager
{
    public void RemoveEntityNode(IEntityNode node)
    {
        shard.EntityRoot.RemoveChild((Node)node);
        pool.ReturnNode(node);
    }
}
