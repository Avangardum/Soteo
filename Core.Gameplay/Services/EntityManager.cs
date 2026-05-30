using System.Collections.Immutable;
using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;
using Soteo.Util;
using Soteo.Util.Interfaces;

namespace Soteo.Core.Gameplay.Services;

public sealed class EntityManager : IEntityManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ClientDependency<ICamera> _camera;
    private readonly IEntityNodeManager _entityNodeManager; 
    
    private readonly Dictionary<Guid, ISnapshottableEntity> _entities = [];
    private readonly List<EntitySnapshot> _deadPuppetSnapshots = [];
    
    public EntityManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _camera = serviceProvider.GetRequiredService<ClientDependency<ICamera>>();
        _entityNodeManager = serviceProvider.GetRequiredService<IEntityNodeManager>();
    }
    
    public ICovariantReadOnlyDictionary<Guid, IEntity> Entities => _entities.AsCovariant();
    
    public event Action<IEntity> EntityAdded = delegate { };
    public event Action<IEntity> EntityRemoved = delegate { };
    
    public IEntity? GetEntity(Guid id) => _entities.GetOrDefault(id);
    
    public IReadOnlyDictionary<Guid, EntitySnapshot> CreateEntityPuppetSnapshots()
    {
        ImmutableDictionary<Guid, EntitySnapshot> snapshots = _entities.Values
            .Select(it => it.CreateSnapshot().ToPuppet())
            .Concat(_deadPuppetSnapshots)
            .ToImmutableDictionary(it => it.Id);
        _deadPuppetSnapshots.Clear();
        return snapshots;
    }
    
    public void ReplicateSnapshot(ShardSnapshot snapshot)
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
            _entities[entitySnapshot.Id].ReplicateSnapshot(entitySnapshot);
        }
    }

    public void ApplyDelta(ShardSnapshotDelta delta, double lerpWeight)
    {
        foreach (EntitySnapshotDelta entityDelta in delta.Entities.Changes.Values)
        {
            bool isNew = !_entities.TryGetValue(entityDelta.Id, out ISnapshottableEntity? entity);
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

    private ISnapshottableEntity SpawnEntityFromSnapshot(EntitySnapshot snapshot)
    {
        return snapshot switch
        {
            UnitPuppetSnapshot s => Add(new UnitPuppet(s.Id, AddNode<IUnitPuppetNode>(s.Id), _camera.Required)),
            ProjectilePuppetSnapshot s =>
                Add(new ProjectilePuppet(s.Id, AddNode<IProjectilePuppetNode>(s.Id), _camera.Required)),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
    
    private ISnapshottableEntity SpawnEntityFromDelta(EntitySnapshotDelta delta)
    {
        return delta switch
        {
            UnitPuppetSnapshotDelta d => Add(new UnitPuppet(d.Id, AddNode<IUnitPuppetNode>(d.Id), _camera.Required)),
            ProjectilePuppetSnapshotDelta d =>
                Add(new ProjectilePuppet(d.Id, AddNode<IProjectilePuppetNode>(d.Id), _camera.Required)),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
    
    public PlayerCharacter SpawnPlayerCharacter(Guid id) =>
        Add(new PlayerCharacter(id, AddNode<IUnitNode>(id), _serviceProvider));

    public Projectile SpawnProjectile(AbilityContext abilityContext, double speed, ProjectileTarget target)
    {
        var id = Guid.NewGuid();
        return Add(new Projectile(id, abilityContext, speed, target, AddNode<IProjectileNode>(id), _serviceProvider)
        {
            // Offset the position 1 pixel up so that the projectile starts behind the source, avoiding 1 frame flicker
            // of the projectile over the source
            Position = abilityContext.User.Position - Vector2.UnitY
        });
    }

    private T Add<T>(T entity) where T : ISnapshottableEntity 
    {
        entity.Removed += () => OnEntityRemoved(entity);
        _entities.Add(entity.Id, entity);
        EntityAdded(entity);
        return entity;
    }
    
    private T AddNode<T>(Guid id) where T : class, IEntityNode =>
        _entityNodeManager.AddNode<T>(id);

    private void OnEntityRemoved(ISnapshottableEntity entity)
    {
        _entityNodeManager.RemoveNode(entity.Id);
        _entities.Remove(entity.Id);
        
        // A final snapshot is sent when a unit dies to notify clients that it's removed due to its death and
        // not for another reason like recall.
        if (entity is Unit { IsDead: true })
            _deadPuppetSnapshots.Add(entity.CreateSnapshot().ToPuppet());
        
        EntityRemoved(entity);
    }
}
