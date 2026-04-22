using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Attributes;
using Soteo.Shared.Extensions;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Nodes.Systems.Synchronization;

public sealed class SynchronizationClient : Node, ISynchronizationPacketReceiver
{
    private IEntityManager _entityManager = null!;
    
    private readonly int _ticksPerSecond;
    private double _tick = -1;
    private long _lastSnapshotTick = -1;
    private readonly ShardSnapshot?[] _snapshotRing;
    private double _second = -1;

    // Stores minimal delta between _tick and _lastSnapshotTick for every recent second. This number
    // shouldn't go below 0, or else synchronization will pause until snapshot necessary to continue arrives. At the
    // same time it's desirable to keep it as low as possible, without risking it going below 0, in order to minimize
    // latency and recover from past pauses caused by one-off network issues. Therefore, if all recent values are above
    // _deltaToLastSnapshotTickMinValueToFastForward, synchronization will fast-forward accordingly.
    private float[] _deltaToLastSnapshotTickHistoryRing = new float[10];
    private readonly float _deltaToLastSnapshotTickMinSafeValue;
    private readonly float _deltaToLastSnapshotTickMinValueToFastForward;
    
    private long SnapshotRingEarliestValidTick => _lastSnapshotTick - _snapshotRing.Length + 1;

    public SynchronizationClient()
    {
        _ticksPerSecond = (int)ProjectSettings.GetSetting("physics/common/physics_fps");
        _snapshotRing = new ShardSnapshot[10 * _ticksPerSecond];
        
        _deltaToLastSnapshotTickMinSafeValue = 0.05f * _ticksPerSecond;
        _deltaToLastSnapshotTickMinValueToFastForward = _deltaToLastSnapshotTickMinSafeValue + 0.01f * _ticksPerSecond;
    }
    
    [Inject]
    public void Inject(IEntityManager entityManager)
    {
        _entityManager = entityManager;
    }
    
    public override void _Ready()
    {
        if (IsServer) QueueFree();
    }

    public override void _Process(float delta)
    {
        if (_tick == -1 && !TryInitialize()) return;
        
        double prevSecondValue = _second;
        _second = _tick / _ticksPerSecond;
        if ((long)_second > (long)prevSecondValue)
            _deltaToLastSnapshotTickHistoryRing.RingSet((long)_second, float.MaxValue);

        if (_tick > _lastSnapshotTick)
        {
            _deltaToLastSnapshotTickHistoryRing.RingSet((long)_second, -1);
            return;
        }

        if (TryGetNearestSnapshotTicks(out int fromTick, out int toTick))
        {
            ShardSnapshot fromSnapshot = _snapshotRing.RingGet(fromTick)!;
            ShardSnapshot toSnapshot = _snapshotRing.RingGet(toTick)!;
            float weight = (float)SoteoMath.InverseLerp(fromTick, toTick, _tick);
            ShardSnapshot interpolatedSnapshot = ShardSnapshot.Interpolate(fromSnapshot, toSnapshot, weight);
            ReplicateSnapshot(interpolatedSnapshot);
        }
        
        WriteDeltaToLastSnapshotTickHistory();
        TryFastForward();
        
        _tick += delta * _ticksPerSecond;
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
        const int maxSkippedTicks = 3;
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
        float deltaToLastSnapshotTick = (float)(_lastSnapshotTick - _tick);
        if (deltaToLastSnapshotTick < _deltaToLastSnapshotTickHistoryRing.RingGet((long)_second))
            _deltaToLastSnapshotTickHistoryRing.RingSet((long)_second, deltaToLastSnapshotTick);
    }

    private void TryFastForward()
    {
        if (_deltaToLastSnapshotTickHistoryRing.All(it => it > _deltaToLastSnapshotTickMinValueToFastForward))
        {
            float minDelta = _deltaToLastSnapshotTickHistoryRing.Min();
            float fastForwardTicks = minDelta - _deltaToLastSnapshotTickMinSafeValue;
            _tick += fastForwardTicks;
            _deltaToLastSnapshotTickHistoryRing.RingSet((long)_second, (float)(_lastSnapshotTick - _tick));
        }
    }
}