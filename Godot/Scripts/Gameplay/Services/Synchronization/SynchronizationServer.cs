using System.Collections.Immutable;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Nodes.Autoloads;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Services.Synchronization;

public sealed class SynchronizationServer : Node, ISynchronizationServer
{
    private readonly IEntityManager _entityManager;
    private readonly IPacketSender _packetSender;

    private long _tick;
    private double _tickInterval;
    private ShardSnapshot? _prevShardSnapshot;
    private readonly HashSet<Guid> _snapshotRequesters = [];
    
    public SynchronizationServer(IEntityManager entityManager, IPacketSender packetSender, IConnectionNotifier connectionNotifier)
    {
        Name = nameof(SynchronizationServer);

        _entityManager = entityManager;
        _packetSender = packetSender;
        connectionNotifier.PeerConnected += OnPeerConnected;

        _tickInterval = 1.0 / (int)ProjectSettings.GetSetting("physics/common/physics_fps");
    }

    private void OnPeerConnected(Guid peerId)
    {
        if (peerId != CampaignServerId)
            _snapshotRequesters.Add(peerId);
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