using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public sealed class MovePacketHandler(IEntitySpawner entitySpawner) : PacketHandler<MovePacket>
{
    protected override void Handle(MovePacket packet, Guid senderId)
    {
        ValidateThisIsServer();
        entitySpawner.GetEntity<Unit>(senderId)?.SetCommand(new MoveCommand(packet.Position));
    }
}