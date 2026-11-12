namespace Soteo.Core.Entities;

public static class EntityManagerExtensions
{
    extension (IEntityManager self)
    {
        public IEntity? GetEntity(Guid id) => self.Entities.GetOrDefault(id);

        public T? GetEntity<T>(Guid id) => (T?)self.GetEntity(id);

        public Projectile SpawnProjectile(AbilityContext abilityContext, double speed, Unit target) =>
            self.SpawnProjectile(abilityContext, speed, new ProjectileTarget(target));
    }
}
