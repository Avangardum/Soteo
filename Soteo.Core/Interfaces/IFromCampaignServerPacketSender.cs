using Soteo.Core.Packets;

namespace Soteo.Core.Interfaces;

public interface IFromCampaignServerPacketSender
{
    void SendTo(Packet packet, Guid receiverId);
    void BroadcastToShardServersAndClients(Packet packet);
    void BroadcastToShardServers(Packet packet);
    void BroadcastToClients(Packet packet);
    void RelayFrom(RelayedPacket packet, Guid senderId);
}
