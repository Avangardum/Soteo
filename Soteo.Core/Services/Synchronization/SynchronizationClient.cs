using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Dto.Snapshots;
using Soteo.Core.Shared.Packets;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Services.Synchronization;

public sealed class SynchronizationClient : ISynchronizationClient, IDisposable
{
    private static readonly double BufferTicksMinSafeValue = 0.05f * Const.TicksPerSecond;
    
    private static readonly double BufferTicksMinValueToFastForward =
        BufferTicksMinSafeValue + 0.01f * Const.TicksPerSecond;
    
    private readonly IEntitySnapshotManager _entitySnapshotManager;
    private readonly IShard _shard;
    private readonly INetworkDebugger _networkDebugger;
    private readonly IFromGameplayPacketSender _packetSender;
    private readonly IDisposable _processSubscription;

    private SynchronizationData _syncData = new();

    public SynchronizationClient
    (
        IEntitySnapshotManager entitySnapshotManager,
        IShard shard,
        INetworkDebugger networkDebugger,
        IFromGameplayPacketSender packetSender,
        IProcessPublisher processPublisher
    )
    {
        _entitySnapshotManager = entitySnapshotManager;
        _shard = shard;
        _networkDebugger = networkDebugger;
        _packetSender = packetSender;
        
        _processSubscription =
            processPublisher.SubscribeToProcess(Process, ProcessPriorityEnum.Default, callWhenPaused: true);
    }
    
    public void Dispose()
    {
        _processSubscription.Dispose();
    }
    
    private StateEnum State
    {
        get;
        set
        {
            field = value;
            if (value == StateEnum.Desynchronized)
            {
                _syncData = new SynchronizationData();
                RequestSnapshot();
            }
        }
    }
    
    public double? ServerLoad => _syncData.ServerLoad;
    
    public double? Latency => (_syncData.ApproxServerTick - _syncData.Tick) / Const.TicksPerSecond;
    
    public int WaitFrameCount { get; private set; }
    public int FastForwardCount { get; private set; }

    private void Process(double delta)
    {
        if (State != StateEnum.Synchronized && !TrySynchronize()) return;
        
        if (_syncData.DeltaRingEarliestValidTick > Maths.CeilToLong(_syncData.Tick!.Value))
        {
            State = StateEnum.Desynchronized;
            return;
        }
        
        _syncData.ApproxServerTick += delta * Const.TicksPerSecond;
        double prevTick = _syncData.Tick!.Value;
        double prevSecond = _syncData.Second!.Value;
        _syncData.Tick += delta * Const.TicksPerSecond;
        if ((long)_syncData.Second > (long)prevSecond)
            _syncData.BufferTicksHistoryRing.RingSet((long)_syncData.Second, double.PositiveInfinity);
        
        if (_syncData.Tick > _syncData.LastDeltaTick)
        {
            _syncData.Tick = prevTick;
            WaitFrameCount++;
            _syncData.BufferTicksHistoryRing.RingSet((long)_syncData.Second, -1);
            return;
        }
        
        ApplyDeltasSince(prevTick);
        WriteBufferTicksHistory();
        TryFastForward();
    }
    
    private void ApplyDeltasSince(double prevTick)
    {
        if (State != StateEnum.Synchronized) throw new InvalidOperationException();
        
        long firstFullDeltaTick = Maths.NextIntegerToLong(prevTick);
        long lastFullDeltaTick = Maths.FloorToLong(_syncData.Tick!.Value);
        
        for (long t = firstFullDeltaTick; t <= lastFullDeltaTick; t++)
            _entitySnapshotManager.ApplyDelta(_syncData.DeltaRing.RingGet(t).Required, 1);

        if (_syncData.Tick % 1 > 0)
        {
            ShardSnapshotDelta partialDelta = _syncData.DeltaRing.RingGet(lastFullDeltaTick + 1).Required;
            if ((long)prevTick < (long)_syncData.Tick)
            {
                double interpolationWeight = _syncData.Tick.Value % 1;
                _entitySnapshotManager.ApplyDelta(partialDelta, interpolationWeight);
            }
            else
            {
                double weight = Maths.InverseLerp(prevTick, lastFullDeltaTick + 1, _syncData.Tick.Value);
                _entitySnapshotManager.ApplyDelta(partialDelta, weight);
            }
        }
    }

    private void RequestSnapshot() =>
        _packetSender.SendReliable(new ShardSnapshotRequestPacket(), _shard.Id);

    private bool TrySynchronize()
    {
        if (State != StateEnum.Synchronizing) return false;
        
        bool isLastSnapshotStale =
            _syncData.DeltaRingEarliestValidTick > _syncData.LastSnapshotPacket?.Snapshot.Tick + 1;
        if (isLastSnapshotStale)
        {
            State = StateEnum.Desynchronized;
            return false;
        }

        bool canSynchronize = _syncData.LastDeltaTick >= _syncData.LastSnapshotPacket?.Snapshot.Tick + 2;
        if (!canSynchronize) return false;
        
        _entitySnapshotManager.ReplicateEntitySnapshots(_syncData.LastSnapshotPacket.Required.Snapshot.Entities);
        _syncData.Tick = _syncData.LastSnapshotPacket.Snapshot.Tick;
        State = StateEnum.Synchronized;
        return true;
    }

    private void WriteBufferTicksHistory()
    {
        if (State != StateEnum.Synchronized) throw new InvalidOperationException();
        
        double bufferTicks = _syncData.LastDeltaTick.Required - _syncData.Tick.Required;
        if (bufferTicks < _syncData.BufferTicksHistoryRing.RingGet((long)_syncData.Second!))
            _syncData.BufferTicksHistoryRing.RingSet((long)_syncData.Second, bufferTicks);
    }

    private void TryFastForward()
    {
        if (State != StateEnum.Synchronized) throw new InvalidOperationException();

        if (_syncData.BufferTicksHistoryRing.Any(it => it < BufferTicksMinValueToFastForward)) return;
        
        double minBufferTicks = _syncData.BufferTicksHistoryRing.Min();
        double fastForwardTicks = minBufferTicks - BufferTicksMinSafeValue;
        _syncData.Tick += fastForwardTicks;
        _syncData.BufferTicksHistoryRing
            .RingSet((long)_syncData.Second!, _syncData.LastDeltaTick.Required - _syncData.Tick.Required);
        FastForwardCount++;
    }
    
    public void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet)
    {
        if (State != StateEnum.Desynchronized) return;
        
        _syncData.LastSnapshotPacket = packet;
        State = StateEnum.Synchronizing;
    }

    public void ReceiveShardSnapshotDeltaPacket(ShardSnapshotDeltaPacket packet)
    {
        if (State == StateEnum.Desynchronized) return;
        
        _syncData.DeltaRing.RingSet(packet.SnapshotDelta.Tick, packet.SnapshotDelta);
        _syncData.ServerLoad = packet.ServerLoad;
        _syncData.ApproxServerTick = packet.SnapshotDelta.Tick + _networkDebugger.Ping(_shard.Id) / 2 * Const.TicksPerSecond;
        _syncData.LastDeltaTick = packet.SnapshotDelta.Tick;
    }
    
    private enum StateEnum
    {
        Desynchronized,
        Synchronizing,
        Synchronized
    }
    
    private record SynchronizationData
    {
        public double? Tick { get; set; }
        public double? ApproxServerTick { get; set; }
        public long? LastDeltaTick { get; set; }
        public ShardSnapshotPacket? LastSnapshotPacket { get; set; }
        public ShardSnapshotDelta?[] DeltaRing { get; } = new ShardSnapshotDelta[10 * Const.TicksPerSecond];
        
        // Stores minimal difference between _tick and _lastDeltaTick for every recent second. This number
        // shouldn't go below 0, or else synchronization will pause until the delta necessary to continue arrives.
        // At the same time it's desirable to keep it as low as possible, without risking it going below 0, in order to
        // minimize latency and recover from past pauses caused by one-off network issues. Therefore, if all recent
        // values are above BufferTicksMinValueToFastForward, synchronization will fast-forward accordingly.
        public double[] BufferTicksHistoryRing { get; } = new double[5];
        
        public double? Second => Tick / Const.TicksPerSecond;
        
        public long? DeltaRingEarliestValidTick => LastDeltaTick - DeltaRing.Length + 1;
        
        public double? ServerLoad { get; set; }
    }
}
