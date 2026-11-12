namespace Soteo.Core.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public sealed class GameplayCampaignInitializedPacketHandler
(
    IGameplayInitPacketReceiver receiver
) : PacketHandler<CampaignInitializedPacket>
{
    protected override void Handle(CampaignInitializedPacket packet, Guid senderId)
    {
        receiver.ReceiveCampaignInitializedPacket();
    }
}
