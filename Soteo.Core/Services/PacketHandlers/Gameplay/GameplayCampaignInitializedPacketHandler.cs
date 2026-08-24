using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

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
