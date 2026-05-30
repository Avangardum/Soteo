using System.Collections.Immutable;
using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Services.Synchronization;

public sealed class SynchronizationServer : ISynchronizationServer, IDisposable
{
    private readonly IEntitySnapshotManager _entitySnapshotManager;
    private readonly IPacketSender _packetSender;
    private readonly IConnectionNotifier _connectionNotifier;
    private readonly IFrameStopwatch _frameStopwatch;

    private long _tick;
    private ShardSnapshot? _prevShardSnapshot;
    private readonly HashSet<Guid> _snapshotRequesters = [];
    private readonly IDisposable _physicsProcessSubscription;
    
    public SynchronizationServer
    (
        IEntitySnapshotManager entitySnapshotManager,
        IPacketSender packetSender,
        IConnectionNotifier connectionNotifier,
        IProcessPublisher processPublisher,
        IFrameStopwatch frameStopwatch
    )
    {
        _entitySnapshotManager = entitySnapshotManager;
        _packetSender = packetSender;
        _connectionNotifier = connectionNotifier;
        _frameStopwatch = frameStopwatch;
        
        connectionNotifier.PeerConnected += OnPeerConnected;
        _physicsProcessSubscription =
            processPublisher.SubscribeToPhysicsProcess(PhysicsProcess, ProcessPriorityEnum.SynchronizationServer);
    }

    public void Dispose()
    {
        _connectionNotifier.PeerConnected -= OnPeerConnected;
        _physicsProcessSubscription.Dispose();
    }

    
    
    private void OnPeerConnected(Guid peerId)
    {
        if (peerId != Const.CampaignServerId)
            _snapshotRequesters.Add(peerId);
    }

    private void PhysicsProcess(double delta)
    {
        var entitySnapshots = _entitySnapshotManager.GetEntityPuppetSnapshots();
        var shardSnapshot = new ShardSnapshot { Entities = entitySnapshots };
        
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
