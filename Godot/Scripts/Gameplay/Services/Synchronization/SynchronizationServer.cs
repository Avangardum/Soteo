using System.Collections.Immutable;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Nodes.Autoloads;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Services.Synchronization;

public sealed class SynchronizationServer : Node
{
    private readonly IEntityManager _entityManager;
    private readonly IPacketSender _packetSender;
    
    private long _tick;
    private double _tickInterval;
    private ShardSnapshot? _prevShardSnapshot;
    
    public SynchronizationServer(IEntityManager entityManager, IPacketSender packetSender)
    {
        Name = nameof(SynchronizationServer);
        
        _entityManager = entityManager;
        _packetSender = packetSender;
        
        _tickInterval = 1.0 / (int)ProjectSettings.GetSetting("physics/common/physics_fps");
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
        ImmutableDictionary<Guid, EntitySnapshot> entitySnapshots = _entityManager.Entities.Values
            .ToImmutableDictionary(it => it.Id, it => it.CreateSnapshot().ToPuppet());
        
        var shardSnapshot = new ShardSnapshot { Entities = entitySnapshots };
        ShardSnapshotDelta? shardSnapshotDelta = _prevShardSnapshot == null ? null :
            ShardSnapshotDelta.Between(_prevShardSnapshot, shardSnapshot);
        _prevShardSnapshot = shardSnapshot;
        double serverLoad = FrameStopwatch.Instance.ElapsedSincePhysicsProcess / _tickInterval;
        var shardSnapshotPacket =
            new ShardSnapshotPacket { Tick = _tick, ServerLoad = serverLoad, Snapshot = shardSnapshot };
        _packetSender.BroadcastReliable(shardSnapshotPacket);
        
        if (shardSnapshotDelta != null)
        {
            var shardSnapshotDeltaPacket = new ShardSnapshotDeltaPacket
            {
                Tick = _tick,
                ServerLoad = serverLoad,
                SnapshotDelta = shardSnapshotDelta
            };
            _packetSender.BroadcastReliable(shardSnapshotDeltaPacket);
        }
        
        _tick++;
    }
}