using Soteo.Core.Dto.Packets;

namespace Soteo.Core.Interfaces;

public interface IFromGameplayToCampaignServerPacketSender
{
    void SendPacket(Packet packet);
}
