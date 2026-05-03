namespace Soteo.Gameplay.Interfaces;

/// <summary>
/// Searches entities by id in all loaded shards
/// </summary>
public interface IEntityLocator
{
    T? FindEntity<T>(Guid entityId, out Guid shardId) where T : class, IEntity;
}