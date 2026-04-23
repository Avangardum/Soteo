using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Nodes.Entities;

// todo remake by sending a callback to the ability
public sealed class AttackProjectile : UnitTargetedProjectile
{
    protected override PackedScene Scene =>
        ResourceLoader.Load<PackedScene>("res://Scenes/Entities/Projectiles/AttackProjectile.tscn"); 

    public AttackProjectile(Guid id, Unit source, Ability ability, ClientDependency<ICamera> camera, Unit target, float speed) :
        base(id, source, ability, speed, camera, target) { }
    
    public AttackProjectile(EntitySnapshot snapshot, ClientDependency<ICamera> camera) :
        this(snapshot.Id, null!, snapshot.Ability!, camera, null!, snapshot.Speed ?? 0) // todo remove redundant !, remove ?? 0
    { } 
    
    protected override void Hit()
    {
        if (Source == null || Target == null) return;
        Source.DealAttackDamageTo(Target, Ability);
    }
}