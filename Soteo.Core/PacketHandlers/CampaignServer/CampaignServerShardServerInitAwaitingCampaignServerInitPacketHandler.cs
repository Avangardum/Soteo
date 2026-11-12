using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.CampaignServer;

[CampaignServerPacketHandler]
public sealed class CampaignServerShardServerInitAwaitingCampaignServerInitPacketHandler
(
    ICampaignServerInitPacketReceiver receiver
) : PacketHandler<ShardServerLocalInitDonePacket>
{
    protected override void Handle(ShardServerLocalInitDonePacket packet, Guid senderId)
    {
        receiver.ReceiveShardServerInitAwaitingCampaignServerInitPacket(senderId);
    }
}
