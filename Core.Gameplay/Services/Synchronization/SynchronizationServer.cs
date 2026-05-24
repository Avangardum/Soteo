using System.Collections.Immutable;
using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Services.Synchronization;

public sealed class SynchronizationServer : ISynchronizationServer, IDisposable
{
    private readonly IEntityManager _entityManager;
    private readonly IPacketSender _packetSender;
    private readonly IConnectionNotifier _connectionNotifier;
    private readonly IFrameStopwatch _frameStopwatch;

    private long _tick;
    private ShardSnapshot? _prevShardSnapshot;
    private readonly HashSet<Guid> _snapshotRequesters = [];
    private readonly List<EntitySnapshot> _entitySnapshots = [];
    private readonly IDisposable _processSubscription;
    
    public SynchronizationServer
    (
        IEntityManager entityManager,
        IPacketSender packetSender,
        IConnectionNotifier connectionNotifier,
        IProcessPublisher processPublisher,
        IFrameStopwatch frameStopwatch
    )
    {
        _entityManager = entityManager;
        _packetSender = packetSender;
        _connectionNotifier = connectionNotifier;
        _frameStopwatch = frameStopwatch;
        
        entityManager.EntityRemoved += OnEntityRemoved;
        connectionNotifier.PeerConnected += OnPeerConnected;
        _processSubscription =
            processPublisher.SubscribeToPhysicsProcess(PhysicsProcess, ProcessPriorityEnum.SynchronizationServer);
    }

    public void Dispose()
    {
        _entityManager.EntityRemoved -= OnEntityRemoved;
        _connectionNotifier.PeerConnected -= OnPeerConnected;
        _processSubscription.Dispose();
    }

    private void OnEntityRemoved(IEntity entity)
    {
        // A final snapshot is sent when a unit dies to notify clients that it's removed due to its death and
        // not for another reason like recall.
        if (entity is Unit { IsDead: true })
            _entitySnapshots.Add(entity.CreateSnapshot().ToPuppet());
    }
    
    private void OnPeerConnected(Guid peerId)
    {
        if (peerId != Const.CampaignServerId)
            _snapshotRequesters.Add(peerId);
    }

    private void PhysicsProcess(double delta)
    {
        foreach (IEntity entity in _entityManager.Entities.Values)
            _entitySnapshots.Add(entity.CreateSnapshot().ToPuppet());
        
        var shardSnapshot = new ShardSnapshot { Entities = _entitySnapshots.ToImmutableDictionary(it => it.Id) };
        _entitySnapshots.Clear();
        
        ShardSnapshotDelta? shardSnapshotDelta = _prevShardSnapshot == null ? null :
            ShardSnapshotDelta.Between(_prevShardSnapshot, shardSnapshot);
        _prevShardSnapshot = shardSnapshot;
        double serverLoad = _frameStopwatch.ElapsedSincePhysicsProcess * Const.TicksPerSecond;

        if (_snapshotRequesters.Count > 0)
        {
            var shardSnapshotPacket = new ShardSnapshotPacket { Tick = _tick, Snapshot = shardSnapshot };
            _packetSender.SendReliable(shardSnapshotPacket, _snapshotRequesters);
            _snapshotRequesters.Clear();
        }

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

    public void ReceiveSnapshotRequest(Guid clientId) => _snapshotRequesters.Add(clientId);
}