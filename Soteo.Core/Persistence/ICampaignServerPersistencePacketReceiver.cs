namespace Soteo.Core.Persistence;

public interface ICampaignServerPersistencePacketReceiver
{
    void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet, Guid senderId);
    void ReceiveShardSnapshotReplicatedPacket(Guid senderId);
}
