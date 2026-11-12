namespace Soteo.Core.PacketHandlers.CampaignServer;

[CampaignServerPacketHandler]
public sealed class CampaignServerShardSnapshotPacketHandler(ICampaignServerPersistencePacketReceiver receiver) :
    PacketHandler<ShardSnapshotPacket>
{
    protected override void Handle(ShardSnapshotPacket packet, Guid senderId) =>
        receiver.ReceiveShardSnapshotPacket(packet, senderId);
}
