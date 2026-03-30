using Soteo.Shared.Packets.PlayerShardServer;

namespace Soteo.Client.PacketHandlers;

public sealed class MovePacketHandler : PacketHandler<MovePacket>
{
    protected override void Handle(MovePacket packet, Guid senderId)
    {
        ValidateThisIsServer();
        GD.Print("Move");
    }
}