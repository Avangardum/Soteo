using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface ISynchronizationClient
{
    float? Latency { get; }
    long SnapshotsReplicated { get; }
    
    void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet);
}