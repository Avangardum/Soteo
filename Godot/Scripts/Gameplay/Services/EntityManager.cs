using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Entities;
using Soteo.Gameplay.EntityNodes;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Services;

public sealed class EntityManager : Node, IEntityManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IShard _shard;
    private readonly IEntityNodePool _entityNodePool;
    private readonly ClientDependency<ICamera> _camera;
    
    private readonly Dictionary<Guid, IEntity> _entities = [];
    private readonly Dictionary<Guid, IEntityNode> _entityNodes = [];
    
    public EntityManager(IServiceProvider serviceProvider)
    {
        Name = nameof(EntityManager);
        
        _serviceProvider = serviceProvider;
        _shard = serviceProvider.GetRequiredService<IShard>();
        _entityNodePool = serviceProvider.GetRequiredService<IEntityNodePool>();
        _camera = serviceProvider.GetRequiredService<ClientDependency<ICamera>>();
    }
    
    public IReadOnlyDictionary<Guid, IEntity> Entities => _entities;
    
    public event Action<IEntity> EntityAdded = delegate {};
    public event Action<IEntity> EntityRemoved = delegate {};

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
            _entities[id].Remove();
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
        return snapshot switch
        {
            UnitSnapshot s => Add(new PlayerCharacter(s, GetUnitNode(snapshot.Id), _serviceProvider)),
            ProjectileSnapshot s => Add(new ProjectilePuppet(s.Id, GetProjectileNode(s.Id), _camera.Required))
        };
    }
    
    public PlayerCharacter SpawnPlayerCharacter(Guid id) =>
        Add(new PlayerCharacter(id, GetUnitNode(id), _serviceProvider));

    public TargetedProjectile SpawnAttackProjectile(AbilityContext abilityContext, double speed)
    {
        var id = Guid.NewGuid();
        return Add(new TargetedProjectile(id, abilityContext, speed, GetProjectileNode(id), _serviceProvider)
        {
            // Offset the position 1 pixel up so that the projectile starts behind the source, avoiding 1 frame flicker
            // of the projectile over the source
            Position = abilityContext.User.Position + Vector2.Up
        });
    }

    private T Add<T>(T entity) where T : IEntity 
    {
        entity.Removed += () => OnEntityRemoved(entity);
        _entities.Add(entity.Id, entity);
        EntityAdded(entity);
        return entity;
    }
    
    private UnitNode GetUnitNode(Guid id)
    {
        UnitNode node = _entityNodePool.GetUnitNode();
        node.Name = $"Unit {id}";
        _shard.EntityRoot.AddChild(node);
        _entityNodes[id] = node;
        return node;
    }
    
    private ProjectileNode GetProjectileNode(Guid id)
    {
        ProjectileNode node = _entityNodePool.GetProjectileNode();
        node.Name = $"Projectile {id}";
        _shard.EntityRoot.AddChild(node);
        _entityNodes[id] = node;
        return node;
    }

    private void OnEntityRemoved(IEntity entity)
    {
        IEntityNode node = _entityNodes[entity.Id];
        _shard.EntityRoot.RemoveChild(node.Node);
        _entityNodePool.ReturnNode(node);
        _entityNodes.Remove(entity.Id);
        _entities.Remove(entity.Id);
        EntityRemoved(entity);
    }
}