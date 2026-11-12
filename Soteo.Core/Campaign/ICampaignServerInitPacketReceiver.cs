namespace Soteo.Core.Campaign;

public interface ICampaignServerInitPacketReceiver
{
    void ReceiveShardServerInitAwaitingCampaignServerInitPacket(Guid senderId);
}
