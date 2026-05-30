using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Gameplay.Entities;
using Soteo.Util.Interfaces;

namespace Soteo.Core.Gameplay.Interfaces;

public interface IEntityManager
{
    ICovariantReadOnlyDictionary<Guid, IEntity> Entities { get; } // todo return IReadOnlyDictionary, add extensions for covariant casting
    
    event Action<IEntity> EntityAdded;
    event Action<IEntity> EntityRemoved;
    
    IReadOnlyDictionary<Guid, EntitySnapshot> CreateEntityPuppetSnapshots(); // todo internal
    void ReplicateSnapshot(ShardSnapshot snapshot); // todo internal, rename and pass only entities
    void ApplyDelta(ShardSnapshotDelta delta, double lerpWeight); // todo internal
    PlayerCharacter SpawnPlayerCharacter(Guid id); // todo internal
    Projectile SpawnProjectile(AbilityContext abilityContext, double speed, ProjectileTarget target); 
    IEntity? GetEntity(Guid id); // todo to extension
}

public static class EntityManagerExtensions
{
    extension (IEntityManager self)
    {
        public T? GetEntity<T>(Guid id) => (T?)self.GetEntity(id);
        
        public Projectile SpawnProjectile(AbilityContext abilityContext, double speed, IUnit target) =>
            self.SpawnProjectile(abilityContext, speed, new ProjectileTarget(target));
    }
}
