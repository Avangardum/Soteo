using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Attributes;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Systems;

public sealed class EntityManager : Node, IEntityManager
{
    private IServiceProvider _serviceProvider = null!;
    private IShard _shard = null!;
    private ClientDependency<ICamera> _camera = null!;
    
    private readonly Dictionary<Guid, IEntity> _entities = [];
    
    [Inject]
    public void Inject(IServiceProvider serviceProvider, IShard shard, ClientDependency<ICamera> camera)
    {
        _serviceProvider = serviceProvider;
        _shard = shard;
        _camera = camera;
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
            if (GetEntity(entitySnapshot.Id) == null) SpawnEntityFromSnapshot(entitySnapshot);
        }
        foreach (Guid id in _entities.Keys.Except(ids).ToArray())
        {
            _entities[id].Node.QueueFree();
        }
        foreach (EntitySnapshot entitySnapshot in snapshot.Entities)
        {
            // Entity snapshots are replicated only after all entities are spawned so that references between entities
            // can be replicated correctly.
            GetEntity(entitySnapshot.Id)!.ReplicateSnapshot(entitySnapshot);
        }
    }
    
    private IEntity SpawnEntityFromSnapshot(EntitySnapshot snapshot)
    {
        // todo detect type from identity
        if (snapshot.Stats.Count > 0)
        {
            return Add(new PlayerCharacter(snapshot, _serviceProvider));
        }
        else
        {
            return Add(new AttackProjectile(snapshot, _camera));
        }
    }
    
    public PlayerCharacter SpawnPlayerCharacter(Guid id) => Add(new PlayerCharacter(id, _serviceProvider));

    public AttackProjectile SpawnAttackProjectile(Unit source, Ability ability, Unit target, float speed)
    {
        // Offset the position 1 pixel up so that the projectile starts behind the source, avoiding 1 frame flicker
        // of the projectile over the source
        Vector2 position = source.Position + Vector2.Up;
        return Add(
            new AttackProjectile(Guid.NewGuid(), source, ability, speed, target, _camera) { Position = position });
    }

    private T Add<T>(T entity) where T : Node2D, IEntity 
    {
        entity.Node.Connect("tree_exited", this, nameof(OnEntityExitedTree), [entity.Id.ToByteArray()]);
        _entities.Add(entity.Id, entity);
        _shard.EntityRoot.AddChild(entity);
        EntityAdded(entity);
        return entity;
    }
    
    public void OnEntityExitedTree(byte[] idBytes)
    {
        var id = new Guid(idBytes);
        _entities.Remove(id);
        EntityRemoved(id);
    }
}