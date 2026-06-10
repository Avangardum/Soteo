using System.Collections.Immutable;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.Services;

public sealed class RoutingPacketSender
(
    IFromGameplayToCampaignServerPacketSender campaignServerSender,
    IFromGameplayPacketSender gameplaySender
) : IFromGameplayPacketSender
{
    public void SendReliable(Packet packet, params IEnumerable<Guid> receiverIds)
    {
        if (receiverIds.Contains(Const.CampaignServerId))
        {
            campaignServerSender.SendPacket(packet);
            gameplaySender.SendReliable(packet, receiverIds.Except([Const.CampaignServerId]).ToImmutableList());
        }
        else
        {
            gameplaySender.SendReliable(packet, receiverIds);
        }
    }

    public void SendUnreliable(Packet packet, params IEnumerable<Guid> receiverIds)
    {
        if (receiverIds.Contains(Const.CampaignServerId))
        {
            campaignServerSender.SendPacket(packet);
            gameplaySender.SendUnreliable(packet, receiverIds.Except([Const.CampaignServerId]).ToImmutableList());
        }
        else
        {
            gameplaySender.SendUnreliable(packet, receiverIds);
        }
    }

    public void BroadcastReliable(Packet packet) => gameplaySender.BroadcastReliable(packet);

    public void BroadcastUnreliable(Packet packet) => gameplaySender.BroadcastUnreliable(packet);
}
