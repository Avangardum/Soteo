using System.Collections.Immutable;
using System.Numerics;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Entities;
using Soteo.Core.Interfaces;
using Soteo.Core.SidedDependencies;

namespace Soteo.Core.Services;

public sealed class EntityManager : IEntityManager, IEntitySnapshotManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ClientDependency<ICamera> _camera;
    private readonly IEntityNodeManager _entityNodeManager; 
    
    private readonly Dictionary<Guid, ISnapshottableEntity> _entities = [];
    
    /// <summary>
    /// Removed entities are stored as WeakReference and are included in persistence snapshots as long as they are
    /// referenced by any other object. That is required to replicate references to removed entities across restarts.
    /// </summary>
    private readonly Dictionary<Guid, WeakReference<ISnapshottableEntity>> _removedEntities = [];
    
    private const int CleanupRemovedEntitiesEveryXRemovals = 1000;
    private int _removalsUntilRemovedEntitiesCleanup = CleanupRemovedEntitiesEveryXRemovals;
    
    /// <summary>
    /// When a unit dies, a puppet snapshot of it is created and sent to clients once to notify them that the unit died
    /// and was not removed for any other reason
    /// </summary>
    private readonly List<EntitySnapshot> _deadPuppetSnapshots = [];
    
    public EntityManager
    (
        IServiceProvider serviceProvider,
        ClientDependency<ICamera> camera,
        IEntityNodeManager entityNodeManager
    )
    {
        _serviceProvider = serviceProvider;
        _camera = camera;
        _entityNodeManager = entityNodeManager;
    }
    
    public IReadOnlyDictionary<Guid, IEntity> Entities =>
        _entities.CovariantCast<Guid, ISnapshottableEntity, IEntity>();
    
    public event Action<IEntity> EntityAdded = delegate { };
    public event Action<IEntity> EntityRemoved = delegate { };
    
    public IReadOnlyDictionary<Guid, EntitySnapshot> CreateEntitySnapshots()
    {
        Dictionary<Guid, EntitySnapshot> snapshots = [];
        
        foreach ((Guid id, ISnapshottableEntity entity) in _entities)
            snapshots[id] = entity.CreateSnapshot();
        
        foreach ((Guid id, WeakReference<ISnapshottableEntity> entityRef) in _removedEntities)
            if (entityRef.TryGetTarget(out ISnapshottableEntity? entity))
                snapshots[id] = entity.CreateSnapshot();
        
        return snapshots;
    }
    
    public IReadOnlyDictionary<Guid, EntitySnapshot> CreateEntityPuppetSnapshots()
    {
        ImmutableDictionary<Guid, EntitySnapshot> snapshots = _entities.Values
            .Select(it => it.CreateSnapshot().ToPuppet())
            .Concat(_deadPuppetSnapshots)
            .ToImmutableDictionary(it => it.Id);
        _deadPuppetSnapshots.Clear();
        return snapshots;
    }
    
    public void ReplicateEntitySnapshots(IReadOnlyDictionary<Guid, EntitySnapshot> snapshots)
    {
        List<Guid> ids = [];
        foreach (EntitySnapshot entitySnapshot in snapshots.Values)
        {
            ids.Add(entitySnapshot.Id);
            if (this.GetEntity(entitySnapshot.Id) == null)
                SpawnEntityFromSnapshot(entitySnapshot);
        }
        foreach (Guid id in _entities.Keys.Except(ids).ToArray())
        {
            _entities[id].Remove();
        }
        foreach (EntitySnapshot entitySnapshot in snapshots.Values)
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
            UnitSnapshot s => Add(PlayerCharacter.FromSnapshot(s, AddNode<IUnitNode>(s.Id), this, _serviceProvider)),
            ProjectileSnapshot s => Add(Projectile.FromSnapshot(s, AddNode<IProjectileNode>(s.Id), _serviceProvider)),
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
    
    public PlayerCharacter SpawnPlayerCharacter(Guid id, Guid controllingPlayerId)
    {
        if (_entities.ContainsKey(id))
            throw new InvalidOperationException("Entity with this id already exists");
        
        if
        (
            _removedEntities.TryGetValue(id, out WeakReference<ISnapshottableEntity> removedEntityRef) &&
            removedEntityRef.TryGetTarget(out ISnapshottableEntity removedEntity)
        )
        {
            if (removedEntity is not PlayerCharacter playerCharacter)
            {
                throw new InvalidOperationException
                (
                    "Attempted respawning a player character from a non player character entity"
                );
            }
            
            if (!playerCharacter.ControllingPlayerIds.SetEquals([controllingPlayerId]))
                throw new InvalidOperationException("Player character control transfer is not supported");

            playerCharacter.Respawn(AddNode<IUnitNode>(id));
            Add(playerCharacter);
            return playerCharacter;
        }
        else
        {
            return Add(new PlayerCharacter(id, controllingPlayerId, AddNode<IUnitNode>(id), this, _serviceProvider));
        }
    }

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
        _removedEntities[entity.Id] = new WeakReference<ISnapshottableEntity>(entity);
        if (--_removalsUntilRemovedEntitiesCleanup == 0)
            CleanupRemovedEntities();
        
        if (entity is Unit { IsDead: true })
            _deadPuppetSnapshots.Add(entity.CreateSnapshot().ToPuppet());
        
        EntityRemoved(entity);
    }
    
    private void CleanupRemovedEntities()
    {
        foreach ((Guid id, WeakReference<ISnapshottableEntity> reference) in _removedEntities.ToDictionary())
            if (!reference.TryGetTarget(out _))
                _removedEntities.Remove(id);
        
        _removalsUntilRemovedEntitiesCleanup = CleanupRemovedEntitiesEveryXRemovals;
    }
}
