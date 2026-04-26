namespace Soteo.Gameplay.Nodes.Entities;

public sealed class AttackProjectile : UnitTargetedProjectile
{
    private static readonly PackedScene Scene =
        ResourceLoader.Load<PackedScene>("res://Scenes/Entities/Projectiles/AttackProjectile.tscn"); 

    public AttackProjectile
    (
        Guid id,
        AbilityContext abilityContext,
        float speed,
        IServiceProvider serviceProvider
    ) : base(id, abilityContext, speed, Scene, serviceProvider) { }
    
    public AttackProjectile(ProjectileSnapshot snapshot, IServiceProvider serviceProvider) :
        this(snapshot.Id, null!, snapshot.Speed, serviceProvider) { } 
}