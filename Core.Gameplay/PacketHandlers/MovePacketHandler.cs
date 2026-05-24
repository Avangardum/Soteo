using Soteo.CampaignServer.PacketHandlers;
using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

[AllowClientPackets]
public sealed class MovePacketHandler(IEntityManager entityManager) : PacketHandler<MovePacket>
{
    protected override void Handle(MovePacket packet, Guid senderId)
    {
        entityManager.GetEntity<Unit>(senderId)?.SetCommand(new MoveCommand(packet.Position));
    }
}