using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Services;

public sealed class EntityManager : IEntityManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ClientDependency<ICamera> _camera;
    private readonly IEntityNodeManager _entityNodeManager; 
    
    private readonly Dictionary<Guid, IEntity> _entities = [];
    
    public EntityManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _camera = serviceProvider.GetRequiredService<ClientDependency<ICamera>>();
        _entityNodeManager = serviceProvider.GetRequiredService<IEntityNodeManager>();
    }
    
    public IReadOnlyDictionary<Guid, IEntity> Entities => _entities;
    
    public event Action<IEntity> EntityAdded = delegate {};
    public event Action<IEntity> EntityRemoved = delegate {};

    public T? GetEntity<T>(Guid id) => (T?)_entities.GetOrDefault(id);
    
    public IEntity? GetEntity(Guid id) => _entities.GetOrDefault(id);
    
    public void ReplicateSnapshotEntities(ShardSnapshot snapshot)
    {
        List<Guid> ids = [];
        foreach (EntitySnapshot entitySnapshot in snapshot.Entities.Values)
        {
            ids.Add(entitySnapshot.Id);
            if (GetEntity(entitySnapshot.Id) == null)
                SpawnEntityFromSnapshot(entitySnapshot);
        }
        foreach (Guid id in _entities.Keys.Except(ids).ToArray())
        {
            _entities[id].Remove();
        }
        foreach (EntitySnapshot entitySnapshot in snapshot.Entities.Values)
        {
            // Entity snapshots are replicated only after all entities are spawned so that references between entities
            // can be replicated correctly.
            GetEntity(entitySnapshot.Id).Required.ReplicateSnapshot(entitySnapshot);
        }
    }

    public void ApplyDelta(ShardSnapshotDelta delta, double lerpWeight)
    {
        foreach (EntitySnapshotDelta entityDelta in delta.Entities.Changes.Values)
        {
            bool isNew = !_entities.TryGetValue(entityDelta.Id, out IEntity? entity);
            if (isNew)
                entity = SpawnEntityFromDelta(entityDelta);
            // Unlike in ReplicateSnapshotEntities, deltas are applied without waiting for all new entities to spawn,
            // because deltas are only used for puppet entities that don't reference other entities.
            entity.Required.ApplyDelta(entityDelta, isNew ? 1 : lerpWeight);
        }
        foreach (Guid id in delta.Entities.RemovedKeys)
        {
            _entities.GetOrDefault(id)?.Remove();
        }
    }

    private IEntity SpawnEntityFromSnapshot(EntitySnapshot snapshot)
    {
        return snapshot switch
        {
            UnitPuppetSnapshot s => Add(new UnitPuppet(s.Id, AddNode<IUnitPuppetNode>(s.Id), _camera.Required)),
            ProjectilePuppetSnapshot s =>
                Add(new ProjectilePuppet(s.Id, AddNode<IProjectilePuppetNode>(s.Id), _camera.Required))
        };
    }
    
    private IEntity SpawnEntityFromDelta(EntitySnapshotDelta delta)
    {
        return delta switch
        {
            UnitPuppetSnapshotDelta d => Add(new UnitPuppet(d.Id, AddNode<IUnitPuppetNode>(d.Id), _camera.Required)),
            ProjectilePuppetSnapshotDelta d =>
                Add(new ProjectilePuppet(d.Id, AddNode<IProjectilePuppetNode>(d.Id), _camera.Required)),
        };
    }
    
    public PlayerCharacter SpawnPlayerCharacter(Guid id) =>
        Add(new PlayerCharacter(id, AddNode<IUnitNode>(id), _serviceProvider));

    public TargetedProjectile SpawnAttackProjectile(AbilityContext abilityContext, double speed)
    {
        var id = Guid.NewGuid();
        return Add(new TargetedProjectile(id, abilityContext, speed, AddNode<IProjectileNode>(id), _serviceProvider)
        {
            // Offset the position 1 pixel up so that the projectile starts behind the source, avoiding 1 frame flicker
            // of the projectile over the source
            Position = abilityContext.User.Position - Vector2.UnitY
        });
    }

    private T Add<T>(T entity) where T : IEntity 
    {
        entity.Removed += () => OnEntityRemoved(entity);
        _entities.Add(entity.Id, entity);
        EntityAdded(entity);
        return entity;
    }
    
    private T AddNode<T>(Guid id) where T : class, IEntityNode =>
        _entityNodeManager.AddNode<T>(id);

    private void OnEntityRemoved(IEntity entity)
    {
        _entityNodeManager.RemoveNode(entity.Id);
        _entities.Remove(entity.Id);
        EntityRemoved(entity);
    }
}
