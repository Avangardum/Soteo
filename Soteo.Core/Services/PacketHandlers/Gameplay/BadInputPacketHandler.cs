using Soteo.Core.Attributes;
using Soteo.Core.Exceptions;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;

[GameplayPacketHandler]
public class BadInputPacketHandler : PacketHandler<BadInputPacket>
{
    protected override void Handle(BadInputPacket packet, Guid senderId)
    {
        throw new BadPacketException(packet.Reason);
    }
}
