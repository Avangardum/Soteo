using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.EntityNodes;

public sealed class UnitNode : KinematicBody2D, IEntityNode
{
    public Node2D Node => this;
        
    public Unit? Unit { get; set; }
    
    public IEntity? Entity
    {
        get => Unit;
        set => Unit = (Unit?)value;
    }
    
    public override void _PhysicsProcess(float delta)
    {
        Unit?.PhysicsProcess(this, delta);
    }
}