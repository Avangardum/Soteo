using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Services;

public sealed class EntityLocator(IShardServiceProviderSource shardServiceProviderSource) : IEntityLocator
{
    public T? FindEntity<T>(Guid entityId, out Guid shardId) where T : class, IEntity
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
        
        shardId = Guid.Empty;
        return null;
    }
}