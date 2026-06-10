using System.Collections.Immutable;
using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Dto.Snapshots;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Services.Synchronization;

public sealed class SynchronizationServer : ISynchronizationServer, IDisposable
{
    private readonly IEntitySnapshotManager _entitySnapshotManager;
    private readonly IFromGameplayPacketSender _packetSender;
    private readonly IConnectionNotifier _connectionNotifier;
    private readonly IFrameStopwatch _frameStopwatch;
    private readonly IPauseRepository _pauseRepo;

    private long _tick;
    private ShardSnapshot? _prevShardSnapshot;
    private readonly HashSet<Guid> _snapshotRequesters = [];
    private readonly IDisposable _physicsProcessSubscription;
    
    public SynchronizationServer
    (
        IEntitySnapshotManager entitySnapshotManager,
        IFromGameplayPacketSender packetSender,
        IConnectionNotifier connectionNotifier,
        IProcessPublisher processPublisher,
        IFrameStopwatch frameStopwatch,
        IPauseRepository pauseRepo
    )
    {
        _entitySnapshotManager = entitySnapshotManager;
        _packetSender = packetSender;
        _connectionNotifier = connectionNotifier;
        _frameStopwatch = frameStopwatch;
        _pauseRepo = pauseRepo;
        
        connectionNotifier.PeerConnected += OnPeerConnected;
        _physicsProcessSubscription = processPublisher
            .SubscribeToPhysicsProcess(Tick, ProcessPriorityEnum.SynchronizationServer, callWhenPaused: true);
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

    private void Tick(double delta)
    {
        var entitySnapshots = _entitySnapshotManager.GetEntityPuppetSnapshots();
        var shardSnapshot = new ShardSnapshot { Tick = _tick, Entities = entitySnapshots };
        
        ShardSnapshotDelta? shardSnapshotDelta = _prevShardSnapshot == null ? null :
            ShardSnapshotDelta.Between(_prevShardSnapshot, shardSnapshot);
        _prevShardSnapshot = shardSnapshot;
        double serverLoad = _frameStopwatch.ElapsedSincePhysicsProcess * Const.TicksPerSecond;

        if (_snapshotRequesters.Count > 0)
        {
            var shardSnapshotPacket = new ShardSnapshotPacket { Snapshot = shardSnapshot };
            _packetSender.SendReliable(shardSnapshotPacket, _snapshotRequesters);
            _snapshotRequesters.Clear();
        }

        if (shardSnapshotDelta != null)
        {
            var shardSnapshotDeltaPacket = new ShardSnapshotDeltaPacket
            {
                ServerLoad = serverLoad,
                SnapshotDelta = shardSnapshotDelta
            };
            _packetSender.BroadcastReliable(shardSnapshotDeltaPacket);
        }

        if (!_pauseRepo.Paused)
            _tick++;
    }

    public void ReceiveSnapshotRequest(Guid clientId) => _snapshotRequesters.Add(clientId);
}
