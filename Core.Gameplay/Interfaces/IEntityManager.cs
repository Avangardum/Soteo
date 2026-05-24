using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Gameplay.Entities;

namespace Soteo.Core.Gameplay.Interfaces;

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