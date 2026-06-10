using Soteo.Core.Shared.Packets;

namespace Soteo.Core.CampaignServer.Interfaces;

public interface IFromCampaignServerPacketSender
{
    void SendTo(Packet packet, Guid receiverId);
    void Broadcast(Packet packet);
    void BroadcastToShardServers(Packet packet);
    void RelayFrom(RelayedPacket packet, Guid senderId);
}
