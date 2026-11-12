namespace Soteo.Core.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public sealed class SynchronizedCampaignStatePacketHandler(ISynchronizedCampaignStatePacketReceiver receiver) :
    PacketHandler<SynchronizedCampaignStatePacket>
{
    protected override void Handle(SynchronizedCampaignStatePacket packet, Guid senderId) =>
        receiver.ReceiveSynchronizedCampaignStatePacket(packet);
}
