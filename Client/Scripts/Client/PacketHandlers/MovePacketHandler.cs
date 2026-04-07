using Soteo.Client.Commands;
using Soteo.Client.Extensions;
using Soteo.Client.Interfaces;
using Soteo.Client.Nodes;
using Soteo.Shared.Packets.PlayerShardServer;

namespace Soteo.Client.PacketHandlers;

public sealed class MovePacketHandler(IEntitySpawner entitySpawner) : PacketHandler<MovePacket>
{
    protected override void Handle(MovePacket packet, Guid senderId)
    {
        ValidateThisIsServer();
        entitySpawner.GetEntity<Unit>(senderId)?.SetCommand(new MoveCommand(packet.Position));
    }
}