namespace Soteo.Gameplay.Interfaces;

public interface IEntitySpawner
{
    IReadOnlyDictionary<Guid, IEntity> Entities { get; }
    
    void SpawnPlayerCharacter(Guid id);
    T? GetEntity<T>(Guid id);
    IEntity? GetEntity(Guid id);
}