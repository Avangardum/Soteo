namespace Soteo.Core.Networking;

public interface IFromCampaignServerPacketSender
{
    void SendTo(Packet packet, params IEnumerable<Guid> receiverIds);
    void BroadcastToAll(Packet packet);
    void BroadcastToShardServers(Packet packet);
    void BroadcastToClients(Packet packet);
    void RelayFrom(RelayedPacket packet, Guid senderId);
}
