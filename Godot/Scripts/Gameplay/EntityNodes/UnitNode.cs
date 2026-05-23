using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.EntityNodes;

public sealed class UnitNode : KinematicBody2D, IUnitNode
{
    public Node2D Node => this;
        
    public Unit? Unit { get; set; }
    
    public IEntity? Entity
    {
        get => Unit;
        set => Unit = (Unit?)value;
    }
    
    public new Vector2 Position
    {
        get => base.Position.ToSys();
        set => base.Position = value.ToGd();
    }

    public void MoveAndCollide(Vector2 movement) => base.MoveAndCollide(movement.ToGd());

    public override void _PhysicsProcess(float delta)
    {
        Unit?.PhysicsProcess(this, delta);
    }
}