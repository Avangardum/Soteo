using System.Collections.Immutable;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Attributes;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Systems;

public sealed class EntityManager : Node, IEntityManager
{
    private IServiceProvider _serviceProvider = null!;
    private IEntityRoots _entityRoots = null!;
    private PackedScene _playerCharacterScene = null!;
    
    private Dictionary<Guid, IEntity> _entities = [];
    
    [Inject]
    public void Inject(IServiceProvider serviceProvider, IEntityRoots entityRoots)
    {
        _serviceProvider = serviceProvider;
        _entityRoots = entityRoots;
    }
    
    public override void _Ready()
    {
        _playerCharacterScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
    }
    
    public IReadOnlyDictionary<Guid, IEntity> Entities => _entities;
    
    public event Action<IEntity> EntityAdded = delegate {};
    public event Action<Guid> EntityRemoved = delegate {};

    public T? GetEntity<T>(Guid id) => (T?)_entities.GetOrDefault(id);
    
    public IEntity? GetEntity(Guid id) => _entities.GetOrDefault(id);
    
    public PlayerCharacter SpawnPlayerCharacter(Guid id)
    {
        var playerCharacter = SpawnEntity<PlayerCharacter>(id, _playerCharacterScene, _entityRoots.PlayerCharacterRoot);
        playerCharacter.DisplayName = id.ToString()[^12..];
        playerCharacter.Inject(_serviceProvider);
        EntityAdded(playerCharacter);
        return playerCharacter;
    }

    private T SpawnEntity<T>(Guid id, PackedScene scene, Node2D root) where T : Node2D, IEntity
    {
        var entity = scene.Instance<T>();
        entity.Node.Name = id.ToString();
        entity.Id = id;
        root.AddChild(entity.Node);
        _entities.Add(id, entity);
        entity.Node.Connect("tree_exited", this, nameof(OnEntityTreeExited), [id.ToByteArray()]);
        return entity;
    }
    
    public void OnEntityTreeExited(byte[] idBytes)
    {
        var id = new Guid(idBytes);
        _entities.Remove(id);
        EntityRemoved(id);
    }
}