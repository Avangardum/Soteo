using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Services;

public sealed class RoutingPacketSender
(
    ICampaignServerCommunicator campaignSender,
    IPacketSender gameplaySender
) : IPacketSender
{
    public void SendReliable(Packet packet, Guid receiverId)
    {
        if (receiverId == CampaignServerId) campaignSender.SendPacket(packet);
        else gameplaySender.SendReliable(packet, receiverId);
    }

    public void SendUnreliable(Packet packet, Guid receiverId)
    {
        if (receiverId == CampaignServerId)
            throw new InvalidOperationException("Campaign server doesn't support unreliable messages");
        gameplaySender.SendUnreliable(packet, receiverId);
    }

    public void SendReliable(Packet packet, IEnumerable<Guid> receiverIds) =>
        gameplaySender.SendReliable(packet, receiverIds);

    public void SendUnreliable(Packet packet, IEnumerable<Guid> receiverIds) =>
        gameplaySender.SendUnreliable(packet, receiverIds);

    public void BroadcastReliable(Packet packet) => gameplaySender.BroadcastReliable(packet);

    public void BroadcastUnreliable(Packet packet) => gameplaySender.BroadcastUnreliable(packet);
}