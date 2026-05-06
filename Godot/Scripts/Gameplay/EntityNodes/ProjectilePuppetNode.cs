using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.EntityNodes;

public sealed class ProjectilePuppetNode : Node2D, IEntityNode
{
    public Node2D Node => this;

    public ProjectilePuppet? ProjectilePuppet { get; set; }
    
    public IEntity? Entity
    {
        get => ProjectilePuppet;
        set => ProjectilePuppet = (ProjectilePuppet?)value;
    }
    
    public EntityProperties Properties { get; private set; } = null!;

    public override void _Ready()
    {
        Properties = GetNode<EntityProperties>("Properties");
    }
}