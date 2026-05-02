using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface ISynchronizationClient
{
    double? Latency { get; }
    
    void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet);
}