using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Nodes.Entities;

// todo remake by sending a callback to the ability
public sealed class AttackProjectile : UnitTargetedProjectile
{
    private static readonly PackedScene Scene =
        ResourceLoader.Load<PackedScene>("res://Scenes/Entities/Projectiles/AttackProjectile.tscn"); 

    public AttackProjectile
    (
        Guid id,
        Unit source,
        Ability ability,
        float speed,
        Unit target,
        ClientDependency<ICamera> camera,
        IShard shard
    ) : base(id, source, ability, speed, target, Scene, camera, shard) { }
    
    public AttackProjectile(ProjectileSnapshot snapshot, ClientDependency<ICamera> camera, IShard shard) :
        this(snapshot.Id, null!, snapshot.Ability!, snapshot.Speed ?? 0, null!, camera, shard) { } // todo remove redundant !, remove ?? 0 
    
    protected override void Hit()
    {
        Source.DealAttackDamageTo(Target, Ability);
    }
}