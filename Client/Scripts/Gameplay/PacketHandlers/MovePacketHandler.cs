using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public sealed class MovePacketHandler(IEntityManager entityManager) : PacketHandler<MovePacket>
{
    protected override void Handle(MovePacket packet, Guid senderId)
    {
        ValidateThisIsServer();
        entityManager.GetEntity<Unit>(senderId)?.SetCommand(new MoveCommand(packet.Position));
    }
}