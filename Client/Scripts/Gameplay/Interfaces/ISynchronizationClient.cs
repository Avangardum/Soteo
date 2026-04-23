using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface ISynchronizationClient
{
    float? Latency { get; }
    
    void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet);
}