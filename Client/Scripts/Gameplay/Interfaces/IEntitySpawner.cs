using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Interfaces;

public interface IEntitySpawner
{
    IReadOnlyDictionary<Guid, IEntity> Entities { get; }
    
    PlayerCharacter SpawnPlayerCharacter(Guid id);
    T? GetEntity<T>(Guid id);
    IEntity? GetEntity(Guid id);
}