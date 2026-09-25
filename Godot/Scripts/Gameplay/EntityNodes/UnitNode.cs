using Soteo.Core.Entities;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;

namespace Soteo.Main.Gameplay.EntityNodes;

public sealed class UnitNode : KinematicBody2D, IUnitNode
{
    public Unit? Unit { get; set; }

    public IEntity? Entity
    {
        get => Unit;
        set => Unit = (Unit?)value;
    }

    public Vector2 PositionMeters
    {
        get => Position.ToSys() / Const.PixelsInMeter;
        set => Position = value.ToGd() * Const.PixelsInMeter;
    }

    public void MoveAndCollide(Vector2 movement) => base.MoveAndCollide(movement.ToGd() * Const.PixelsInMeter);

    public override void _PhysicsProcess(float delta)
    {
        Unit?.Tick(delta);
    }
}
