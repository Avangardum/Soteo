namespace Soteo.Core.Entities;

public static class EntityManagerExtensions
{
    extension (IEntityManager self)
    {
        public Entity? GetEntity(Guid id) => self.Entities.GetValueOrDefault(id);

        public T? GetEntity<T>(Guid id) where T : Entity => (T?)self.GetEntity(id);

        public Projectile SpawnProjectile(AbilityContext abilityContext, double speed, Unit target) =>
            self.SpawnProjectile(abilityContext, speed, new ProjectileTarget(target));
    }
}
