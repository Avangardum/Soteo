using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Extensions;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Services.Synchronization;

public sealed class SynchronizationClient : Node, ISynchronizationClient
{
    private readonly IEntityManager _entityManager;
    private readonly IShard _shard;
    private readonly INetworkDebugger _networkDebugger;
    
    private readonly int _ticksPerSecond;
    private double _tick = -1;
    private double _serverTick = -1;
    private long _lastSnapshotTick = -1;
    private readonly ShardSnapshot?[] _snapshotRing;
    private double _second = -1;

    // Stores minimal delta between _tick and _lastSnapshotTick for every recent second. This number
    // shouldn't go below 0, or else synchronization will pause until snapshot necessary to continue arrives. At the
    // same time it's desirable to keep it as low as possible, without risking it going below 0, in order to minimize
    // latency and recover from past pauses caused by one-off network issues. Therefore, if all recent values are above
    // _deltaToLastSnapshotTickMinValueToFastForward, synchronization will fast-forward accordingly.
    private readonly double[] _deltaToLastSnapshotTickHistoryRing = new double[5];
    private readonly double _deltaToLastSnapshotTickMinSafeValue;
    private readonly double _deltaToLastSnapshotTickMinValueToFastForward;
    
    private readonly double[] _serverLoadRing;
    
    public IReadOnlyList<double> ServerLoadHistory => _serverLoadRing.UnrollRing(_lastSnapshotTick + 1);
    
    public double? Latency =>
        _tick == -1 || _serverTick == -1 ? null : (_serverTick - _tick) / _ticksPerSecond;
    
    public int WaitFrameCount { get; private set; }
    public int FastForwardCount { get; private set; }

    private long SnapshotRingEarliestValidTick => _lastSnapshotTick - _snapshotRing.Length + 1;

    public SynchronizationClient(IEntityManager entityManager, IShard shard, INetworkDebugger networkDebugger)
    {
        Name = nameof(SynchronizationClient);
        
        _entityManager = entityManager;
        _shard = shard;
        _networkDebugger = networkDebugger;
        
        _ticksPerSecond = (int)ProjectSettings.GetSetting("physics/common/physics_fps");
        _snapshotRing = new ShardSnapshot[10 * _ticksPerSecond];
        _serverLoadRing = new double[_snapshotRing.Length];
        
        _deltaToLastSnapshotTickMinSafeValue = 0.05f * _ticksPerSecond;
        _deltaToLastSnapshotTickMinValueToFastForward = _deltaToLastSnapshotTickMinSafeValue + 0.01f * _ticksPerSecond;
    }
    
    public override void _Ready()
    {
        if (IsServer) QueueFree();
    }

    public override void _Process(float delta)
    {
        if (_tick == -1 && !TryInitialize()) return;
        
        if (_serverTick != -1)
            _serverTick += delta * _ticksPerSecond;
        
        if (_tick > _lastSnapshotTick)
        {
            WaitFrameCount++;
            _deltaToLastSnapshotTickHistoryRing.RingSet((long)_second, -1);
            return;
        }
        
        double prevSecondValue = _second;
        _tick += delta * _ticksPerSecond;
        _second = _tick / _ticksPerSecond;
        if ((long)_second > (long)prevSecondValue)
            _deltaToLastSnapshotTickHistoryRing.RingSet((long)_second, double.MaxValue);

        if (TryGetNearestSnapshotTicks(out int fromTick, out int toTick))
        {
            ShardSnapshot fromSnapshot = _snapshotRing.RingGet(fromTick).Required;
            ShardSnapshot toSnapshot = _snapshotRing.RingGet(toTick).Required;
            double weight = Maths.InverseLerp(fromTick, toTick, _tick);
            ShardSnapshot interpolatedSnapshot = fromSnapshot.Interpolate(toSnapshot, weight);
            ReplicateSnapshot(interpolatedSnapshot);
        }
        
        WriteDeltaToLastSnapshotTickHistory();
        TryFastForward();
    }
    
    private bool TryInitialize()
    {
        if
        (
            _snapshotRing.RingGet(_lastSnapshotTick) != null &&
            _snapshotRing.RingGet(_lastSnapshotTick - 1) != null &&
            _snapshotRing.RingGet(_lastSnapshotTick - 2) != null
        )
        {
            _tick = _lastSnapshotTick - 2;
            return true;
        }
        return false;
    }
    
    private bool TryGetNearestSnapshotTicks(out int fromTick, out int toTick)
    {
        int maxSkippedTicks = _ticksPerSecond;
        int skippedTicks = 0;
        fromTick = (int)Math.Floor(_tick);
        toTick = -1;
        while (_snapshotRing.RingGet(fromTick) == null)
        {
            if (++skippedTicks > maxSkippedTicks) return false;
            if (--fromTick < SnapshotRingEarliestValidTick) return false;
        }
        toTick = (int)Math.Ceiling(_tick);
        while (_snapshotRing.RingGet(toTick) == null)
        {
            if (++skippedTicks > maxSkippedTicks) return false;
            if (++toTick > _lastSnapshotTick) return false;
        }
        return true;
    }

    public void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet)
    {
        _snapshotRing.RingSet(packet.Tick, packet.Snapshot);
        _serverLoadRing.RingSet(packet.Tick, packet.ServerLoad);
        double? halfPingTicks = _networkDebugger.Ping(_shard.Id) * _ticksPerSecond / 2;
        _serverTick = halfPingTicks == null ? -1 : packet.Tick + halfPingTicks.Value;
        
        if (_lastSnapshotTick != -1 && packet.Tick > _lastSnapshotTick && _lastSnapshotTick != packet.Tick - 1)
        {
            for (long t = _lastSnapshotTick + 1; t < packet.Tick; t++)
            {
                _snapshotRing.RingSet(t, null);
            }
        }
        
        if (packet.Tick > _lastSnapshotTick) _lastSnapshotTick = packet.Tick;
    }
    
    private void ReplicateSnapshot(ShardSnapshot snapshot)
    {
        _entityManager.ReplicateSnapshotEntities(snapshot);
    }

    private void WriteDeltaToLastSnapshotTickHistory()
    {
        double deltaToLastSnapshotTick = (_lastSnapshotTick - _tick);
        if (deltaToLastSnapshotTick < _deltaToLastSnapshotTickHistoryRing.RingGet((long)_second))
            _deltaToLastSnapshotTickHistoryRing.RingSet((long)_second, deltaToLastSnapshotTick);
    }

    private void TryFastForward()
    {
        if (_deltaToLastSnapshotTickHistoryRing.All(it => it > _deltaToLastSnapshotTickMinValueToFastForward))
        {
            double minDelta = _deltaToLastSnapshotTickHistoryRing.Min();
            double fastForwardTicks = minDelta - _deltaToLastSnapshotTickMinSafeValue;
            _tick += fastForwardTicks;
            _deltaToLastSnapshotTickHistoryRing.RingSet((long)_second, _lastSnapshotTick - _tick);
            FastForwardCount++;
        }
    }
}