using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Gameplay.Entities;

namespace Soteo.Core.Gameplay.Interfaces;

public interface IEntityManager
{
    IReadOnlyDictionary<Guid, IEntity> Entities { get; }
    
    event Action<IEntity> EntityAdded;
    event Action<IEntity> EntityRemoved;
    
    void ReplicateSnapshot(ShardSnapshot snapshot);
    void ApplyDelta(ShardSnapshotDelta delta, double lerpWeight);
    PlayerCharacter SpawnPlayerCharacter(Guid id);
    Projectile SpawnProjectile(AbilityContext abilityContext, double speed);
    IEntity? GetEntity(Guid id);
}

public static class EntityManagerExtensions
{
    extension (IEntityManager self)
    {
        public T? GetEntity<T>(Guid id) => (T?)self.GetEntity(id);
    }
}
