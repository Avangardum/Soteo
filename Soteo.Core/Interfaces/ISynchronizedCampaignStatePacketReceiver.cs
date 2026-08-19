using Soteo.Core.Dto.Packets;

namespace Soteo.Core.Interfaces;

public interface ISynchronizedCampaignStatePacketReceiver
{
    void ReceiveSynchronizedCampaignStatePacket(SynchronizedCampaignStatePacket packet);
}
