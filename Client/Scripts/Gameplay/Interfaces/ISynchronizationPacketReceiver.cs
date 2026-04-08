using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface ISynchronizationPacketReceiver
{
    void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet);
}