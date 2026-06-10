using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services;

/// <inheritdoc />
public sealed class EntityLocator(IShardServiceProviders shardServiceProviders) : IEntityLocator
{
    public bool TryFindEntity<T>
    (
        Guid entityId,
        [NotNullWhen(true)] out T? entity,
        [NotNullWhen(true)] out Guid? shardId
    ) where T : class, IEntity
    {
        foreach ((Guid currentShardId, IServiceProvider services) in shardServiceProviders)
        {
            T? currentShardEntity = services.GetRequiredService<IEntityManager>().GetEntity<T>(entityId);
            if (currentShardEntity != null)
            {
                entity = currentShardEntity;
                shardId = currentShardId;
                return true;
            }
        }
        
        entity = null;
        shardId = null;
        return false;
    }
}
