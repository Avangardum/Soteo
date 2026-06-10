using System.Diagnostics.CodeAnalysis;

namespace Soteo.Core.Interfaces;

/// <summary>
/// Searches entities by id in all loaded shards
/// </summary>
public interface IEntityLocator
{
    bool TryFindEntity<T>(Guid entityId, [NotNullWhen(true)] out T? entity, [NotNullWhen(true)] out Guid? shardId)
        where T : class, IEntity;
}
