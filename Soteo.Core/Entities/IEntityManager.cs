namespace Soteo.Core.Entities;

public interface IEntityManager
{
    IReadOnlyDictionary<Guid, Entity> Entities { get; }

    event Action<Entity> EntityAdded;
    event Action<Entity> EntityRemoved;

    PlayerCharacter SpawnPlayerCharacter(Guid id, Guid controllingPlayerId);
    Projectile SpawnProjectile(AbilityContext abilityContext, double speed, ProjectileTarget target);
    Dude SpawnDude(Vector2 position);
    Turret SpawnTurret(Vector2 position);
}

public static class EntityManagerExtensions
{
    extension(IEntityManager self)
    {
        public Entity? GetEntity(Guid id) => self.Entities.GetValueOrDefault(id);
        public T? GetEntity<T>(Guid id) where T : Entity => (T?)self.GetEntity(id);
    }
}
