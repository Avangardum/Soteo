using Soteo.Core.Dto.Packets;

namespace Soteo.Core.Interfaces;

public interface IShardSnapshotPacketReceiver
{
    void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet, Guid senderId);
}
