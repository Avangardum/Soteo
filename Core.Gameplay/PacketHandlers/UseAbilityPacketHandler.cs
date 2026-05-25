using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;

namespace Soteo.Core.Gameplay.PacketHandlers;

[AllowClientPackets]
public sealed class UseAbilityPacketHandler(IEntityManager entityManager) : PacketHandler<UseAbilityPacket>
{
    protected override void Handle(UseAbilityPacket packet, Guid senderId)
    {
        entityManager.GetEntity<Unit>(senderId)?.SetCommand(packet.Command);
    }
}
