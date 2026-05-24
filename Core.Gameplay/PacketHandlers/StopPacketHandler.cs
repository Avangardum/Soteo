using Soteo.CampaignServer.PacketHandlers;
using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

[AllowClientPackets]
public sealed class StopPacketHandler(IEntityManager entityManager) : PacketHandler<StopPacket>
{
    protected override void Handle(StopPacket packet, Guid senderId)
    {
        entityManager.GetEntity<Unit>(senderId)?.CancelCommands();
    }
}
