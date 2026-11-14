using Soteo.Core;
using Soteo.Core.Entities;
using Soteo.Core.Entities.Projectiles;

namespace Soteo.Main.Gameplay.EntityNodes;

public sealed partial class ProjectileNode : Area2D, IProjectileNode
{
    public Projectile? Projectile { get; set; }

    public IEntity? Entity
    {
        get => Projectile;
        set => Projectile = (Projectile?)value;
    }

    public Vector2 PositionMeters
    {
        get => Position.ToSys() / Const.PixelsInMeter;
        set => Position = value.ToGd() * Const.PixelsInMeter;
    }
}
