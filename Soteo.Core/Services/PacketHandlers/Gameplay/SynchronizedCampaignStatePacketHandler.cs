using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public sealed class SynchronizedCampaignStatePacketHandler(ISynchronizedCampaignStatePacketReceiver receiver) :
    PacketHandler<SynchronizedCampaignStatePacket>
{
    protected override void Handle(SynchronizedCampaignStatePacket packet, Guid senderId) =>
        receiver.ReceiveSynchronizedCampaignStatePacket(packet);
}
