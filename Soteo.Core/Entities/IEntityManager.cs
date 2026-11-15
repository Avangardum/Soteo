namespace Soteo.Core.Entities;

public interface IEntityManager
{
    IReadOnlyDictionary<Guid, Entity> Entities { get; }

    event Action<Entity> EntityAdded;
    event Action<Entity> EntityRemoved;

    PlayerCharacter SpawnPlayerCharacter(Guid id, Guid controllingPlayerId);
    Projectile SpawnProjectile(AbilityContext abilityContext, double speed, ProjectileTarget target);
}
