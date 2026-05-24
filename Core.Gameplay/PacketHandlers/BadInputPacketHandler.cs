using Soteo.Core.Shared;
using Soteo.Core.Shared.Exceptions;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.PacketHandlers;

public class BadInputPacketHandler : PacketHandler<BadInputPacket>
{
    protected override void Handle(BadInputPacket packet, Guid senderId)
    {
        throw new BadPacketException(packet.Reason);
    }
}