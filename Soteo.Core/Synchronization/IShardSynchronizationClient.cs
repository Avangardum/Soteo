namespace Soteo.Core.Synchronization;

public interface IShardSynchronizationClient
{
    double? Latency { get; }
    int WaitFrameCount { get; }
    int FastForwardCount { get; }
    double? ServerLoad { get; }

    void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet);
    void ReceiveShardSnapshotDeltaPacket(ShardSnapshotDeltaPacket packet);
}
