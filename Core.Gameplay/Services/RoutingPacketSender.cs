using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared.Packets;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Services;

public sealed class RoutingPacketSender
(
    ICampaignServerCommunicator campaignSender,
    IPacketSender gameplaySender
) : IPacketSender
{
    public void SendReliable(Packet packet, Guid receiverId)
    {
        if (receiverId == Const.CampaignServerId)
            campaignSender.SendPacket(packet);
        else gameplaySender.SendReliable(packet, receiverId);
    }

    public void SendUnreliable(Packet packet, Guid receiverId)
    {
        if (receiverId == Const.CampaignServerId)
            campaignSender.SendPacket(packet);
        gameplaySender.SendUnreliable(packet, receiverId);
    }

    public void SendReliable(Packet packet, IEnumerable<Guid> receiverIds) =>
        gameplaySender.SendReliable(packet, receiverIds);

    public void SendUnreliable(Packet packet, IEnumerable<Guid> receiverIds) =>
        gameplaySender.SendUnreliable(packet, receiverIds);

    public void BroadcastReliable(Packet packet) => gameplaySender.BroadcastReliable(packet);

    public void BroadcastUnreliable(Packet packet) => gameplaySender.BroadcastUnreliable(packet);
}
