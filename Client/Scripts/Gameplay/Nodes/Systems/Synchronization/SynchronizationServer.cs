using System.Collections.Immutable;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Nodes.Systems.Synchronization;

public sealed class SynchronizationServer : Node
{
    private readonly IEntityManager _entityManager;
    private readonly IPacketSender _packetSender;
    
    private long _tick;
    
    public SynchronizationServer(IEntityManager entityManager, IPacketSender packetSender)
    {
        Name = nameof(SynchronizationServer);
        
        _entityManager = entityManager;
        _packetSender = packetSender;
    }
    
    public override void _Ready()
    {
        if (!IsServer)
        {
            SetPhysicsProcess(false);
            QueueFree();
        }

        ProcessPriority = (int)ProcessPriorityEnum.SynchronizationServer;
    }

    public override void _PhysicsProcess(float delta)
    {
        ImmutableList<EntitySnapshot> entitySnapshots = _entityManager.Entities.Values
            .Select(it => it.CreateSnapshot())
            .ToImmutableList();
        
        var shardSnapshot = new ShardSnapshot { Entities = entitySnapshots };
        var packet = new ShardSnapshotPacket { Tick = _tick, Snapshot = shardSnapshot };
        _packetSender.BroadcastUnreliable(packet);
        
        _tick++;
    }
}