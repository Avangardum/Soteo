using Soteo.Gameplay.EntityNodes;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Services;

public sealed class EntityNodePool : IEntityNodePool
{
    private static readonly PackedScene UnitScene =
        ResourceLoader.Load<PackedScene>("res://Scenes/Entities/Unit.tscn");
    
    private static readonly PackedScene ProjectileScene =
        ResourceLoader.Load<PackedScene>("res://Scenes/Entities/Projectile.tscn");
    
    private const int PreloadedUnitCount = 1000;
    private const int PreloadedProjectileCount = 1000;
    
    private readonly Stack<UnitNode> _unitNodes = new(PreloadedUnitCount);
    private readonly Stack<ProjectileNode> _projectileNodes = new(PreloadedProjectileCount);

    public EntityNodePool()
    {
        for (int i = 0; i < PreloadedUnitCount; i++)
            _unitNodes.Push(UnitScene.Instance<UnitNode>());
        
        for (int i = 0; i < PreloadedProjectileCount; i++)
            _projectileNodes.Push(ProjectileScene.Instance<ProjectileNode>());
    }
    
    public UnitNode GetUnitNode()
    {
        if (_unitNodes.Count > 0)
            return _unitNodes.Pop();
        return UnitScene.Instance<UnitNode>();
    }

    public ProjectileNode GetProjectileNode()
    {
        if (_projectileNodes.Count > 0)
            return _projectileNodes.Pop();
        return ProjectileScene.Instance<ProjectileNode>();
    }

    public void ReturnNode(IEntityNode node)
    {
        switch (node)
        {
            case UnitNode unit:
                _unitNodes.Push(unit);
                break;
            case ProjectileNode projectile:
                _projectileNodes.Push(projectile);
                break;
            default:
                throw new ArgumentException($"Unknown node type {node.GetType()}");
        }
    }
}