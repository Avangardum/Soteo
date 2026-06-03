using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Interfaces;

public interface ICampaignServerPacketSender
{
    void SendPacket(Packet packet);
}
