using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Entities;

namespace Soteo.Gameplay.Interfaces;

public interface IEntityManager
{
    IReadOnlyDictionary<Guid, IEntity> Entities { get; }
    
    event Action<IEntity> EntityAdded;
    event Action<IEntity> EntityRemoved;
    
    void ReplicateSnapshotEntities(ShardSnapshot snapshot);
    void ApplyDelta(ShardSnapshotDelta delta, double lerpWeight);
    PlayerCharacter SpawnPlayerCharacter(Guid id);
    TargetedProjectile SpawnAttackProjectile(AbilityContext abilityContext, double speed);
    T? GetEntity<T>(Guid id);
    IEntity? GetEntity(Guid id);
}