using Soteo.Core.Entities;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;

namespace Soteo.Main.Gameplay.EntityNodes;

public sealed class ProjectileNode : Area2D, IProjectileNode
{
    public Projectile? Projectile { get; set; }

    public IEntity? Entity
    {
        get => Projectile;
        set => Projectile = (Projectile?)value;
    }

    public Vector2 PositionM
    {
        get => Position.ToSys() / Const.PixelsInMeter;
        set => Position = value.ToGd() * Const.PixelsInMeter;
    }

    public override void _PhysicsProcess(float delta)
    {
        Projectile?.Tick(delta);
    }
}
