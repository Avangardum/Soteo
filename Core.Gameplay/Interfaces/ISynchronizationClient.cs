using Soteo.Core.Gameplay.Packets;

namespace Soteo.Core.Gameplay.Interfaces;

public interface ISynchronizationClient
{
    double? Latency { get; }
    int WaitFrameCount { get; }
    int FastForwardCount { get; }
    double? ServerLoad { get; }
    
    void ReceiveShardSnapshotPacket(SynchronizationShardSnapshotPacket packet);
    void ReceiveShardSnapshotDeltaPacket(ShardSnapshotDeltaPacket packet);
}
