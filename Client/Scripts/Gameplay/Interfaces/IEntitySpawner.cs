namespace Soteo.Gameplay.Interfaces;

public interface IEntitySpawner
{
    void SpawnPlayerCharacter(Guid id);
    T? GetEntity<T>(Guid id) where T : Node2D;
}