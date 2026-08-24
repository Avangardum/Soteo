using Soteo.Core.Dto.Packets;

namespace Soteo.Core.Interfaces;

public interface ICampaignServerInitPacketReceiver
{
    void ReceiveShardServerInitAwaitingCampaignServerInitPacket(Guid senderId);
}
