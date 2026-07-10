using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.CampaignServer;

[CampaignServerPacketHandler]
public sealed class CampaignServerShardSnapshotReplicatedPacketHandler
(
    ICampaignServerPersistencePacketReceiver receiver
) : PacketHandler<ShardSnapshotReplicatedPacket>
{
    protected override void Handle(ShardSnapshotReplicatedPacket packet, Guid senderId) =>
        receiver.ReceiveShardSnapshotReplicatedPacket(senderId);
}
