using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Interfaces;

public interface IFromGameplayToCampaignServerPacketSender
{
    void SendPacket(Packet packet);
}
