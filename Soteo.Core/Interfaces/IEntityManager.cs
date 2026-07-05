using Soteo.Core.Dto;
using Soteo.Core.Entities;

namespace Soteo.Core.Interfaces;

public interface IEntityManager
{
    IReadOnlyDictionary<Guid, IEntity> Entities { get; }
        
    event Action<IEntity> EntityAdded;
    event Action<IEntity> EntityRemoved;
        
    PlayerCharacter SpawnPlayerCharacter(Guid id, Guid controllingPlayerId);
    Projectile SpawnProjectile(AbilityContext abilityContext, double speed, ProjectileTarget target); 
}