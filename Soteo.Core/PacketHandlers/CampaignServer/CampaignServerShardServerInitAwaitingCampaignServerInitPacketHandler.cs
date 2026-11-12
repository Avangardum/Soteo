namespace Soteo.Core.PacketHandlers.CampaignServer;

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
