using Soteo.Shared.Exceptions;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public class BadInputPacketHandler : PacketHandler<BadInputPacket>
{
    protected override void Handle(BadInputPacket packet, Guid senderId)
    {
        Validate(!IsServer || senderId == MasterServerId, "Client can't report bad packets");
        throw new BadPacketException(packet.Reason);
    }
}