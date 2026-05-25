using Soteo.Core.Gameplay.Commands;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;

namespace Soteo.Core.Gameplay.PacketHandlers;

[AllowClientPackets]
public sealed class MovePacketHandler(IEntityManager entityManager) : PacketHandler<MovePacket>
{
    protected override void Handle(MovePacket packet, Guid senderId)
    {
        entityManager.GetEntity<Unit>(senderId)?.SetCommand(new MoveCommand(packet.Position));
    }
}
