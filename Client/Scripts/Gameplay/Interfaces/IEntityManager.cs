using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Interfaces;

public interface IEntityManager
{
    IReadOnlyDictionary<Guid, IEntity> Entities { get; }
    
    event Action<IEntity> EntityAdded;
    event Action<Guid> EntityRemoved;
    
    void ReplicateSnapshotEntities(ShardSnapshot snapshot);
    PlayerCharacter SpawnPlayerCharacter(Guid id);
    AttackProjectile SpawnAttackProjectile(Unit source, Ability ability, Unit target, float speed);
    T? GetEntity<T>(Guid id);
    IEntity? GetEntity(Guid id);
}