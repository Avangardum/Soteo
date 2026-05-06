using System.Collections.Immutable;
using Soteo.Gameplay.EntityNodes;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Services;

public sealed class EntityNodePool : IEntityNodePool
{
    private static readonly ImmutableDictionary<Type, PackedScene> Scenes = new Dictionary<Type, string>
    {
        [typeof(UnitNode)] = "res://Scenes/Entities/Unit.tscn",
        [typeof(ProjectileNode)] = "res://Scenes/Entities/Projectile.tscn",
        [typeof(UnitPuppetNode)] = "res://Scenes/Entities/UnitPuppet.tscn",
        [typeof(ProjectilePuppetNode)] = "res://Scenes/Entities/ProjectilePuppet.tscn",
    }.ToImmutableDictionary(it => it.Key, it => ResourceLoader.Load<PackedScene>(it.Value));
    
    private readonly ImmutableDictionary<Type, Stack<IEntityNode>> _stacks =
        Scenes.ToImmutableDictionary(it => it.Key, _ => new Stack<IEntityNode>());

    public T GetNode<T>() where T : Node2D, IEntityNode
    {
        Stack<IEntityNode> stack = _stacks[typeof(T)];
        if (stack.Count > 0)
            return (T)stack.Pop();
        return Scenes[typeof(T)].Instance<T>();
    }
    
    public void ReturnNode(IEntityNode node)
    {
        _stacks[node.GetType()].Push(node);
    }
}