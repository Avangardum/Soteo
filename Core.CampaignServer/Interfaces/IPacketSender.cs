using Soteo.Shared.Packets;

namespace Soteo.CampaignServer.Interfaces;

public interface IPacketSender
{
    void SendTo(Packet packet, Guid receiverId);
    
    void RelayFrom(RelayedPacket packet, Guid senderId);
}