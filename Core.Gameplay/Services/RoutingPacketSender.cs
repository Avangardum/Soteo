using System.Collections.Immutable;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Packets;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Services;

public sealed class RoutingPacketSender
(
    ICampaignServerCommunicator campaignSender,
    IPacketSender gameplaySender
) : IPacketSender
{
    public void SendReliable(Packet packet, params IEnumerable<Guid> receiverIds)
    {
        if (receiverIds.Contains(Const.CampaignServerId))
        {
            campaignSender.SendPacket(packet);
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
            campaignSender.SendPacket(packet);
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
