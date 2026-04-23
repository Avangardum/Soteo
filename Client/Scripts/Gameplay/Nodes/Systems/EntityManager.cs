using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared;
using Soteo.Shared.Attributes;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Systems;

public sealed class EntityManager : Node, IEntityManager
{
    private IServiceProvider _serviceProvider = null!;
    private IShard _shard = null!;
    private ClientDependency<ICamera> _camera = null!;
    
    private PackedScene _playerCharacterScene = null!;
    
    private Dictionary<Guid, IEntity> _entities = [];
    
    [Inject]
    public void Inject(IServiceProvider serviceProvider, IShard shard, ClientDependency<ICamera> camera)
    {
        _serviceProvider = serviceProvider;
        _shard = shard;
        _camera = camera;
    }
    
    public override void _Ready()
    {
        _playerCharacterScene = GD.Load<PackedScene>(Scenes.Player);
    }
    
    public IReadOnlyDictionary<Guid, IEntity> Entities => _entities;
    
    public event Action<IEntity> EntityAdded = delegate {};
    public event Action<Guid> EntityRemoved = delegate {};

    public T? GetEntity<T>(Guid id) => (T?)_entities.GetOrDefault(id);
    
    public IEntity? GetEntity(Guid id) => _entities.GetOrDefault(id);
    
    public void ReplicateSnapshotEntities(ShardSnapshot snapshot)
    {
        List<Guid> ids = [];
        foreach (EntitySnapshot entitySnapshot in snapshot.Entities)
        {
            ids.Add(entitySnapshot.Id);
            IEntity entity = GetEntity(entitySnapshot.Id) ?? SpawnEntityFromSnapshot(entitySnapshot);
            entity.ReplicateSnapshot(entitySnapshot);
            // todo defer snapshot replication to after all new entities are spawned to ensure entity references
            // replicate correctly
        }
        foreach (Guid id in _entities.Keys.Except(ids).ToArray())
        {
            _entities[id].Node.QueueFree();
        }
    }
    
    private IEntity SpawnEntityFromSnapshot(EntitySnapshot snapshot)
    {
        // todo detect type from identity
        if (snapshot.Stats.Count > 0)
        {
            // todo use special constructor
            var playerCharacter = SpawnPlayerCharacter(snapshot.Id);
            playerCharacter.ReplicateSnapshot(snapshot);
            return playerCharacter;
        }
        else
        {
            return SpawnAttackProjectile(snapshot);
        }
    }
    
    // todo use SpawnEntityV2
    public PlayerCharacter SpawnPlayerCharacter(Guid id)
    {
        var playerCharacter = SpawnEntity<PlayerCharacter>(id, _playerCharacterScene, _shard.PlayerCharacterRoot);
        playerCharacter.DisplayName = id.ToString()[^12..];
        playerCharacter.Inject(_serviceProvider);
        EntityAdded(playerCharacter);
        return playerCharacter;
    }
    
    public AttackProjectile SpawnAttackProjectile(Unit source, Ability ability, Unit target, float speed)
    {
        // Offset the position 1 pixel up so that the projectile starts behind the source, avoiding 1 frame flicker
        // of the projectile over the source
        // todo move to Projectile constructor after source is properly injected
        Vector2 position = source.Position + Vector2.Up;
        return AddEntity(
            new AttackProjectile(Guid.NewGuid(), source, ability, _camera, target, speed) { Position = position });
    }
    
    public AttackProjectile SpawnAttackProjectile(EntitySnapshot snapshot)
    {
        return AddEntity(new AttackProjectile(snapshot, _camera));
    }
    
    private T AddEntity<T>(T entity) where T : Node2D, IEntity 
    {
        entity.Node.Connect("tree_exited", this, nameof(OnEntityExitedTree), [entity.Id.ToByteArray()]);
        _entities.Add(entity.Id, entity);
        _shard.PlayerCharacterRoot.AddChild(entity); // todo leave 1 entity root
        EntityAdded(entity);
        return entity;
    }

    private T SpawnEntity<T>(Guid id, PackedScene scene, Node2D root) where T : Node2D, IEntity
    {
        var entity = scene.Instance<T>();
        entity.Node.Name = id.ToString();
        entity.Id = id;
        root.AddChild(entity.Node);
        _entities.Add(id, entity);
        entity.Node.Connect("tree_exited", this, nameof(OnEntityExitedTree), [id.ToByteArray()]);
        return entity;
    }
    
    public void OnEntityExitedTree(byte[] idBytes)
    {
        var id = new Guid(idBytes);
        _entities.Remove(id);
        EntityRemoved(id);
    }
}