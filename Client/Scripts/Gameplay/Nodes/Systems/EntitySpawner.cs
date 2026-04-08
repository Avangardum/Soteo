using System.Collections.Immutable;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Systems;

public sealed class EntitySpawner : Node, IEntitySpawner
{
    private IEntityRoots _entityRoots = null!;
    private PackedScene _playerCharacterScene = null!;
    
    private Dictionary<Guid, IEntity> _entities = [];
    
    [Inject]
    public void Inject(IEntityRoots entityRoots)
    {
        _entityRoots = entityRoots;
    }
    
    public override void _Ready()
    {
        _playerCharacterScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
    }
    
    public IReadOnlyDictionary<Guid, IEntity> Entities => _entities;

    public T? GetEntity<T>(Guid id) => (T?)_entities.GetOrDefault(id);
    
    public IEntity? GetEntity(Guid id) => _entities.GetOrDefault(id);
    
    public void SpawnPlayerCharacter(Guid id) =>
        SpawnEntity(id, _playerCharacterScene, _entityRoots.PlayerCharacterRoot);
    
    private void SpawnEntity(Guid id, PackedScene scene, Node2D root)
    {
        var entity = scene.Instance<IEntity>();
        entity.Node.Name = id.ToString();
        root.AddChild(entity.Node);
        _entities.Add(id, entity);
        entity.Node.Connect("tree_exited", this, nameof(OnEntityTreeExited), [id.ToByteArray()]);
    }
    
    public void OnEntityTreeExited(byte[] id)
    {
        _entities.Remove(new Guid(id));
    }
}