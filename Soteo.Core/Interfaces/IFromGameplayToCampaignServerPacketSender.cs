using Soteo.Core.Packets;

namespace Soteo.Core.Interfaces;

public interface IFromGameplayToCampaignServerPacketSender
{
    void SendPacket(Packet packet);
}
