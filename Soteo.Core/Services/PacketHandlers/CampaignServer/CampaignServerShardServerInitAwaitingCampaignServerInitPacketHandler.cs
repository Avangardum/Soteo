using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.CampaignServer;

[CampaignServerPacketHandler]
public sealed class CampaignServerShardServerInitAwaitingCampaignServerInitPacketHandler
(
    ICampaignServerInitPacketReceiver receiver
) : PacketHandler<ShardServerInitAwaitingCampaignServerInitPacket>
{
    protected override void Handle(ShardServerInitAwaitingCampaignServerInitPacket packet, Guid senderId)
    {
        receiver.ReceiveShardServerInitAwaitingCampaignServerInitPacket(senderId);
    }
}
