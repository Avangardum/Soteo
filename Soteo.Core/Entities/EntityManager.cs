namespace Soteo.Core.Entities;

public sealed class EntityManager : IEntityManager, IEntitySnapshotManager, IDisposable
{
    private readonly ServerDependency<Services> _services;
    private readonly ClientDependency<ICamera> _camera;
    private readonly IEntityNodeManager _entityNodeManager;
    private readonly ICrossInventoryHelper _crossInventoryHelper;

    private readonly IDisposable _processSubscription;
    private readonly SortedDictionary<Guid, Entity> _entities = [];
    private Entity[] _entityIterationBuffer = new Entity[1024];

    /// <summary>
    /// Removed entities are stored as WeakReference and are included in persistence snapshots as long as they are
    /// referenced by any other object. That is required to replicate references to removed entities across restarts.
    /// </summary>
    private readonly Dictionary<Guid, WeakReference<Entity>> _removedEntities = [];

    private const int CleanupRemovedEntitiesEveryXRemovals = 1000;
    private int _removalsUntilRemovedEntitiesCleanup = CleanupRemovedEntitiesEveryXRemovals;

    /// <summary>
    /// When a unit dies, a puppet snapshot of it is created and sent to clients once to notify them that the unit died
    /// and was not removed for any other reason
    /// </summary>
    private readonly List<EntitySnapshot> _deadPuppetSnapshots = [];

    public EntityManager
    (
        ServerDependency<Services> services,
        ClientDependency<ICamera> camera,
        IEntityNodeManager entityNodeManager,
        ICrossInventoryHelper crossInventoryHelper,
        IProcessPublisher processPublisher,
        SideOptions sideOptions
    )
    {
        _services = services;
        _camera = camera;
        _entityNodeManager = entityNodeManager;
        _crossInventoryHelper = crossInventoryHelper;

        _processSubscription = sideOptions.Side is Side.Client ?
            processPublisher.SubscribeToProcess(Tick, ProcessPriority.Default, callWhenPaused: false) :
            processPublisher.SubscribeToPhysicsProcess(Tick, ProcessPriority.Default, callWhenPaused: false);
    }

    public void Dispose()
    {
        _processSubscription.Dispose();
    }

    public IReadOnlyDictionary<Guid, Entity> Entities => _entities;

    public event Action<Entity> EntityAdded = delegate { };
    public event Action<Entity> EntityRemoved = delegate { };

    private void Tick(double delta)
    {
        while (_entityIterationBuffer.Length < _entities.Count)
            Array.Resize(ref _entityIterationBuffer, _entityIterationBuffer.Length * 2);
        _entities.Values.CopyTo(_entityIterationBuffer, 0);
        foreach (Entity entity in _entityIterationBuffer.AsSpan(.._entities.Count))
            entity.Tick(delta);
    }

    public IReadOnlyDictionary<Guid, EntitySnapshot> CreateEntitySnapshots()
    {
        Dictionary<Guid, EntitySnapshot> snapshots = [];

        foreach ((Guid id, Entity entity) in _entities)
            snapshots[id] = entity.ToSnapshot();

        foreach ((Guid id, WeakReference<Entity> entityRef) in _removedEntities)
            if (entityRef.TryGetTarget(out Entity? entity))
                snapshots[id] = entity.ToSnapshot();

        return snapshots;
    }

    public IReadOnlyDictionary<Guid, EntitySnapshot> CreateEntityPuppetSnapshots()
    {
        IReadOnlyDictionary<Guid, EntitySnapshot> snapshots = _entities.Values
            .Select(it => it.ToSnapshot().ToPuppet())
            .Concat(_deadPuppetSnapshots)
            .ToDictionary(it => it.Id).AsReadOnly();
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

    public void ApplyDelta(ShardDelta delta, double lerpWeight)
    {
        foreach (EntityDelta entityDelta in delta.Entities.Changes.Values)
        {
            bool isNew = !_entities.TryGetValue(entityDelta.Id, out Entity? entity);
            if (isNew)
                entity = SpawnEntityFromDelta(entityDelta);
            // Unlike in ReplicateSnapshotEntities, deltas are applied without waiting for all new entities to spawn,
            // because deltas are only used for puppet entities that don't reference other entities.
            entity.Required.ApplyDelta(entityDelta, isNew ? 1 : lerpWeight);
        }
        foreach (Guid id in delta.Entities.RemovedKeys)
        {
            _entities.GetValueOrDefault(id)?.Remove();
        }
    }

    // todo OCP violation: adding unit types requires updating EntityManager

    private Entity SpawnEntityFromSnapshot(EntitySnapshot snapshot)
    {
        return snapshot switch
        {
            UnitSnapshot s when s.Type == typeof(PlayerCharacter)  => Add
            (
                new PlayerCharacter
                (
                    s.Id,
                    AddNode<IUnitNode>(s.Id),
                    _services.Required,
                    s.ControllingPlayerId.Required
                )
            ),
            UnitSnapshot s when s.Type == typeof(Dude) =>
                Add(new Dude(s.Id, AddNode<IUnitNode>(s.Id), _services.Required)),
            ProjectileSnapshot s => Add(Projectile.FromSnapshot(s, _services.Required)),
            UnitPuppetSnapshot s => Add
            (
                new UnitPuppet
                (
                    s.Id,
                    AddNode<IUnitPuppetNode>(s.Id),
                    _camera.Required
                )
            ),
            ProjectilePuppetSnapshot s =>
                Add(new ProjectilePuppet(s.Id, AddNode<IProjectilePuppetNode>(s.Id), _camera.Required)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Entity SpawnEntityFromDelta(EntityDelta delta)
    {
        return delta switch
        {
            UnitPuppetDelta d => Add
            (
                new UnitPuppet
                (
                    d.Id,
                    AddNode<IUnitPuppetNode>(d.Id),
                    _camera.Required
                )
            ),
            ProjectilePuppetDelta d =>
                Add(new ProjectilePuppet(d.Id, AddNode<IProjectilePuppetNode>(d.Id), _camera.Required)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public PlayerCharacter SpawnPlayerCharacter(Guid id, Guid controllingPlayerId)
    {
        if (_entities.ContainsKey(id))
            throw new InvalidOperationException("Entity with this id already exists");

        if
        (
            _removedEntities.TryGetValue(id, out WeakReference<Entity>? removedEntityRef) &&
            removedEntityRef.TryGetTarget(out Entity? removedEntity)
        )
        {
            if (removedEntity is not PlayerCharacter playerCharacter)
            {
                throw new InvalidOperationException
                (
                    "Attempted respawning a player character from a non player character entity"
                );
            }

            if (!playerCharacter.GetControllingPlayerIds().SequenceEqual([controllingPlayerId]))
                throw new InvalidOperationException("Player character control transfer is not supported");

            playerCharacter.Respawn(AddNode<IUnitNode>(id));
            Add(playerCharacter);
            return playerCharacter;
        }
        else
        {
            return Add
            (
                new PlayerCharacter
                (
                    id,
                    AddNode<IUnitNode>(id),
                    _services.Required,
                    controllingPlayerId
                )
            );
        }
    }

    public Projectile SpawnProjectile(AbilityContext abilityContext, double speed, ProjectileTarget target)
    {
        var id = Guid.NewGuid();
        return Add(new Projectile(id, abilityContext, speed, target, _services.Required)
        {
            // Offset the position 1 pixel up so that the projectile starts behind the source, avoiding 1 frame flicker
            // of the projectile over the source
            Position = abilityContext.User.Position - Vector2.UnitY
        });
    }

    public Dude SpawnDude(Vector2 position)
    {
        var id = Guid.NewGuid();
        return Add
        (
            new Dude(id, AddNode<IUnitNode>(id), _services.Required) { Position = position }
        );
    }

    private T Add<T>(T entity) where T : Entity
    {
        entity.Removed += () => OnEntityRemoved(entity);
        _entities.Add(entity.Id, entity);
        EntityAdded(entity);
        return entity;
    }

    private T AddNode<T>(Guid id) where T : class, IEntityNode =>
        _entityNodeManager.AddNode<T>(id);

    private void OnEntityRemoved(Entity entity)
    {
        _entityNodeManager.RemoveNode(entity.Id);
        _entities.Remove(entity.Id);
        _removedEntities[entity.Id] = new WeakReference<Entity>(entity);
        if (--_removalsUntilRemovedEntitiesCleanup == 0)
            CleanupRemovedEntities();

        if (entity is Unit { IsDead: true })
            _deadPuppetSnapshots.Add(entity.ToSnapshot().ToPuppet());

        EntityRemoved(entity);
    }

    private void CleanupRemovedEntities()
    {
        foreach ((Guid id, WeakReference<Entity> reference) in _removedEntities.ToDictionary())
            if (!reference.TryGetTarget(out _))
                _removedEntities.Remove(id);

        _removalsUntilRemovedEntitiesCleanup = CleanupRemovedEntitiesEveryXRemovals;
    }
}
