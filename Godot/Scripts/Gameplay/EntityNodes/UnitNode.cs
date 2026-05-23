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

    public void MoveAndCollide(GdVector2 movement) => base.MoveAndCollide(movement);

    public override void _PhysicsProcess(float delta)
    {
        Unit?.PhysicsProcess(this, delta);
    }
}