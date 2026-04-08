using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Nodes.Systems.Synchronization;

public sealed class SynchronizationClient : Node, ISynchronizationPacketReceiver
{
    private IEntitySpawner _entitySpawner = null!;
    
    private long _tick;
    
    [Inject]
    public void Inject(IEntitySpawner entitySpawner)
    {
        _entitySpawner = entitySpawner;
    }
    
    public override void _Ready()
    {
        if (IsServer) QueueFree();
    }
    
    public void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet)
    {
        ReplicateSnapshot(packet.Snapshot);
    }
    
    private void ReplicateSnapshot(ShardSnapshot snapshot)
    {
        IEnumerable<Guid> oldEntityIds = _entitySpawner.Entities.Keys;
        IEnumerable<Guid> newEntityIds = snapshot.Entities.Select(it => it.Id);
        IEnumerable<Guid> removedEntityIds = oldEntityIds.Except(newEntityIds);
        IEnumerable<Guid> addedEntityIds = newEntityIds.Except(oldEntityIds);
        
        foreach (Guid id in removedEntityIds)
        {
            _entitySpawner.GetEntity<Node2D>(id)!.QueueFree();
        }
        foreach (Guid id in addedEntityIds)
        {
            _entitySpawner.SpawnPlayerCharacter(id);
        }
        foreach (EntitySnapshot entitySnapshot in snapshot.Entities)
        {
            IEntity entity = _entitySpawner.GetEntity(entitySnapshot.Id)!;
            if (entitySnapshot.Position != null) entity.Position = entitySnapshot.Position.Value;
            if (entitySnapshot.Azimuth != null) entity.Azimuth = entitySnapshot.Azimuth.Value;
        }
    }
}