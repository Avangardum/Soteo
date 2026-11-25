using Soteo.Core;
using Soteo.Core.Entities;
using Soteo.Core.Entities.Units;

namespace Soteo.Main.Gameplay.EntityNodes;

public sealed partial class UnitNode : CharacterBody2D, IUnitNode
{
    private NavigationAgent2D _navigationAgent = null!;

    public override void _Ready()
    {
        _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
    }

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

    public Vector2 NextPathPosition(Vector2 target)
    {
        double targetDesiredDistance = _navigationAgent.TargetDesiredDistance / Const.PixelsInMeter;
        if (Vector2.Distance(PositionMeters, target) <= targetDesiredDistance) return target;

        Vector2 prevTarget = _navigationAgent.TargetPosition.ToSys() / Const.PixelsInMeter;

        if
        (
            _navigationAgent.IsNavigationFinished() ||
            Vector2.Distance(prevTarget, target) > targetDesiredDistance
        )
        {
            _navigationAgent.TargetPosition = target.ToGd() * Const.PixelsInMeter;
        }

        return _navigationAgent.GetNextPathPosition().ToSys() / Const.PixelsInMeter;
    }

    public bool IsNavigationFinished => _navigationAgent.IsNavigationFinished();
}
