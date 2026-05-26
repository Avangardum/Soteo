using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.EntityNodes;

public sealed class ProjectileNode : Area2D, IProjectileNode
{
    public Projectile? Projectile { get; set; }
    
    public IEntity? Entity
    {
        get => Projectile;
        set => Projectile = (Projectile?)value;
    }

    public new Vector2 Position
    {
        get => base.Position.ToSys();
        set => base.Position = value.ToGd();
    }

    public override void _PhysicsProcess(float delta)
    {
        Projectile?.Tick(delta);
    }
}
