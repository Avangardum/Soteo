using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Exceptions;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public class BadInputPacketHandler : PacketHandler<BadInputPacket>
{
    protected override void Handle(BadInputPacket packet, Guid senderId)
    {
        throw new BadPacketException(packet.Reason);
    }
}
