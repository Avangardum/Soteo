namespace Soteo.Core.PacketHandlers.CampaignServer;

[CampaignServerPacketHandler]
public sealed class ShardSnapshotReplicatedPacketHandler
(
    ICampaignServerPersistencePacketReceiver receiver
) : PacketHandler<ShardSnapshotReplicatedPacket>
{
    protected override void Handle(ShardSnapshotReplicatedPacket packet, Guid senderId) =>
        receiver.ReceiveShardSnapshotReplicatedPacket(senderId);
}
