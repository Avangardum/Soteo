using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Core.Gameplay.Services;

/// <inheritdoc />
public sealed class EntityLocator(IShardServiceProviderSource shardServiceProviderSource) : IEntityLocator
{
    public T? FindEntity<T>(Guid entityId, out Guid? shardId) where T : class, IEntity
    {
        foreach ((Guid currentShardId, IServiceProvider services) in shardServiceProviderSource.ShardServiceProviders)
        {
            T? entity = services.GetRequiredService<IEntityManager>().GetEntity<T>(entityId);
            if (entity != null)
            {
                shardId = currentShardId;
                return entity;
            }
        }
        
        shardId = null;
        return null;
    }
}
