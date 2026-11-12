namespace Soteo.Core.Campaign;

public interface ISynchronizedCampaignStatePacketReceiver
{
    void ReceiveSynchronizedCampaignStatePacket(SynchronizedCampaignStatePacket packet);
}
