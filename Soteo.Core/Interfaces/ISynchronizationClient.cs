using Soteo.Core.Packets;

namespace Soteo.Core.Interfaces;

public interface ISynchronizationClient
{
    double? Latency { get; }
    int WaitFrameCount { get; }
    int FastForwardCount { get; }
    double? ServerLoad { get; }
    
    void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet);
    void ReceiveShardSnapshotDeltaPacket(ShardSnapshotDeltaPacket packet);
}
