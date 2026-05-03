using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface ISynchronizationClient
{
    double? Latency { get; }
    int WaitFrameCount { get; }
    int FastForwardCount { get; }
    
    void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet);
}