namespace Soteo.Core.Networking;

public sealed class RoutingPacketSender
(
    IFromGameplayToCampaignServerPacketSender campaignServerSender,
    IFromGameplayPacketSender gameplaySender
) : IFromGameplayPacketSender
{
    public void Send(Packet packet, IReadOnlyList<Guid> receiverIds, bool reliable = true, bool deferToNextTick = false)
    {
        if (receiverIds.Contains(Const.CampaignServerId))
        {
            campaignServerSender.SendPacket(packet);
            gameplaySender
                .Send(packet, receiverIds.Except([Const.CampaignServerId]).ToList(), reliable, deferToNextTick);
        }
        else
        {
            gameplaySender.Send(packet, receiverIds, reliable, deferToNextTick);
        }
    }

    public void Broadcast(Packet packet, bool reliable = true, bool deferToNextTick = false) =>
        gameplaySender.Broadcast(packet, reliable, deferToNextTick);
}
