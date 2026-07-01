using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.CampaignServer;

[CampaignServerPacketHandler]
public sealed class CampaignServerShardSnapshotPacketHandler(IShardSnapshotPacketReceiver receiver) :
    PacketHandler<ShardSnapshotPacket>
{
    protected override void Handle(ShardSnapshotPacket packet, Guid senderId) =>
        receiver.ReceiveShardSnapshotPacket(packet, senderId);
}
