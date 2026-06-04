using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Core.Gameplay.Interfaces
{
    public interface IEntityManager
    {
        IReadOnlyDictionary<Guid, IEntity> Entities { get; }
        
        event Action<IEntity> EntityAdded;
        event Action<IEntity> EntityRemoved;
        
        PlayerCharacter SpawnPlayerCharacter(Guid id, Guid controllingPlayerId);
        Projectile SpawnProjectile(AbilityContext abilityContext, double speed, ProjectileTarget target); 
    }
}

namespace Soteo.Core.Gameplay.Extensions
{
    public static class EntityManagerExtensions
    {
        extension (IEntityManager self)
        {
            public IEntity? GetEntity(Guid id) => self.Entities.GetOrDefault(id);
            
            public T? GetEntity<T>(Guid id) => (T?)self.GetEntity(id);
            
            public Projectile SpawnProjectile(AbilityContext abilityContext, double speed, IUnit target) =>
                self.SpawnProjectile(abilityContext, speed, new ProjectileTarget(target));
        }
    }
}
