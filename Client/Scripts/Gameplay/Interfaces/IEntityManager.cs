using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Interfaces;

public interface IEntityManager
{
    IReadOnlyDictionary<Guid, IEntity> Entities { get; }
    
    event Action<IEntity> EntityAdded;
    event Action<Guid> EntityRemoved;
    
    PlayerCharacter SpawnPlayerCharacter(Guid id);
    T? GetEntity<T>(Guid id);
    IEntity? GetEntity(Guid id);
}