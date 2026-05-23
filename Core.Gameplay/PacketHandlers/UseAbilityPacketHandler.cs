using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

[AllowClientPackets]
public sealed class UseAbilityPacketHandler(IEntityManager entityManager) : PacketHandler<UseAbilityPacket>
{
    protected override void Handle(UseAbilityPacket packet, Guid senderId)
    {
        entityManager.GetEntity<Unit>(senderId)?.SetCommand(packet.Command);
    }
}