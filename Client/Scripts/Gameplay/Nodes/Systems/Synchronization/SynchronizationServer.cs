using System.Collections.Immutable;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Nodes.Systems.Synchronization;

public sealed class SynchronizationServer : Node
{
    private IEntitySpawner _entitySpawner = null!;
    private IPacketSender _packetSender = null!;
    
    private long _tick;
    
    [Inject]
    public void Inject(IEntitySpawner entitySpawner, IPacketSender packetSender)
    {
        _entitySpawner = entitySpawner;
        _packetSender = packetSender;
    }
    
    public override void _Ready()
    {
        if (!IsServer)
        {
            Free();
            return;
        }
        ProcessPriority = (int)ProcessPriorityEnum.ReplicatorServer;
    }

    public override void _PhysicsProcess(float delta)
    {
        ImmutableList<EntitySnapshot> entitySnapshots = _entitySpawner.Entities.Values
            .Select(entity => new EntitySnapshot
            {
                Id = entity.Id,
                Position = entity.Position,
                Azimuth = entity.Azimuth
            })
            .ToImmutableList();
        
        var shardSnapshot = new ShardSnapshot { Entities = entitySnapshots };
        var packet = new ShardSnapshotPacket { Tick = _tick, Snapshot = shardSnapshot };
        _packetSender.BroadcastUnreliable(packet);
        
        _tick++;
    }
}