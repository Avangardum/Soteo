using Soteo.Core.Entities;
using Soteo.Core.Entities.Projectiles;
using Soteo.Core.Entities.Units;
using Soteo.Main.Gameplay.Interfaces;

namespace Soteo.Main.Gameplay.Services;

public sealed class EntityNodePool : IEntityNodePool
{
    private static readonly IReadOnlyDictionary<Type, PackedScene> Scenes = new Dictionary<Type, string>
    {
        [typeof(IUnitNode)] = "res://Scenes/Entities/Unit.tscn",
        [typeof(IProjectileNode)] = "res://Scenes/Entities/Projectile.tscn",
        [typeof(IUnitPuppetNode)] = "res://Scenes/Entities/UnitPuppet.tscn",
        [typeof(IProjectilePuppetNode)] = "res://Scenes/Entities/ProjectilePuppet.tscn"
    }.ToDictionary(it => it.Key, it => ResourceLoader.Load<PackedScene>(it.Value)).AsReadOnly();

    private readonly IReadOnlyDictionary<Type, Stack<IEntityNode>> _stacks =
        Scenes.ToDictionary(it => it.Key, _ => new Stack<IEntityNode>()).AsReadOnly();

    public T GetNode<T>() where T : class, IEntityNode
    {
        Stack<IEntityNode> stack = _stacks[typeof(T)];
        if (stack.Count > 0)
            return (T)stack.Pop();
        return Scenes[typeof(T)].Instantiate<T>();
    }

    public void ReturnNode(IEntityNode node)
    {
        foreach ((Type type, Stack<IEntityNode> stack) in _stacks)
        {
            if (node.GetType().IsAssignableTo(type))
            {
                stack.Push(node);
                return;
            }
        }
        throw new ArgumentException($"Unknown entity node type {node.GetType()}");
    }
}
