using System.Collections.Generic;
using Soteo.Client.Interfaces;
using Soteo.Shared.Extensions;

namespace Soteo.Client.Nodes.Systems;

public sealed class EntitySpawner : Node, IEntitySpawner
{
    private IEntityRoots _entityRoots = null!;
    private PackedScene _playerCharacterScene = null!;
    
    private Dictionary<Guid, Node2D> _entities = [];
    
    public void Inject(IEntityRoots entityRoots)
    {
        _entityRoots = entityRoots;
    }
    
    public override void _Ready()
    {
        _playerCharacterScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
    }

    public T? GetEntity<T>(Guid id) where T : Node2D => (T?)_entities.GetOrDefault(id);
    
    public void SpawnPlayerCharacter(Guid id) =>
        SpawnEntity(id, _playerCharacterScene, _entityRoots.PlayerCharacterRoot);
    
    private void SpawnEntity(Guid id, PackedScene scene, Node2D root)
    {
        var entity = scene.Instance<Node2D>();
        entity.Name = id.ToString();
        root.AddChild(entity);
        _entities.Add(id, entity);
        entity.Connect("tree_exited", this, nameof(OnEntityTreeExited), [id.ToByteArray()]);
    }
    
    public void OnEntityTreeExited(byte[] id)
    {
        _entities.Remove(new Guid(id));
    }
}