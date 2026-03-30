using Soteo.Shared.Packets.PlayerShard;

namespace Soteo.Client.PacketHandlers;

public sealed class MovePacketHandler : PacketHandler<MovePacket>
{
    protected override void Handle(MovePacket packet, Guid senderId)
    {
        ValidateThisIsServer();
        GD.Print("Move");
    }
}