using Soteo.Core.Dto.Packets;

namespace Soteo.Core.Interfaces;

public interface ICampaignServerPersistencePacketReceiver
{
    void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet, Guid senderId);
    void ReceiveShardSnapshotReplicatedPacket(Guid senderId);
}
