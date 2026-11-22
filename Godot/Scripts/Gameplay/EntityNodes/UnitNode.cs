using Soteo.Core;
using Soteo.Core.Entities;
using Soteo.Core.Entities.Units;

namespace Soteo.Main.Gameplay.EntityNodes;

public sealed partial class UnitNode : CharacterBody2D, IUnitNode
{
    public Unit? Unit { get; set; }

    public Entity? Entity
    {
        get => Unit;
        set => Unit = (Unit?)value;
    }

    public Vector2 PositionMeters
    {
        get => Position.ToSys() / Const.PixelsInMeter;
        set => Position = value.ToGd() * Const.PixelsInMeter;
    }

    public void Move(Vector2 movement)
    {
        Velocity = movement.ToGd() * Const.PixelsInMeter / Const.TickInterval;
        MoveAndSlide();
    }
}
