using Soteo.Core.Gameplay.Packets;

namespace Soteo.Core.Gameplay.Interfaces;

public interface ISynchronizationClient
{
    double? Latency { get; }
    int WaitFrameCount { get; }
    int FastForwardCount { get; }
    IReadOnlyList<double> ServerLoadHistory { get; }
    
    void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet);
    void ReceiveShardSnapshotDeltaPacket(ShardSnapshotDeltaPacket packet);
}