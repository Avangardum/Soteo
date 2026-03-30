using Soteo.Shared.Exceptions;
using Soteo.Shared.Packets.Master;

namespace Soteo.Client.PacketHandlers;

public class BadInputPacketHandler : PacketHandler<BadInputPacket>
{
    protected override void Handle(BadInputPacket packet, Guid senderId)
    {
        Validate(!IsServer || senderId == MasterServerId, "Clients can't report bad input");
        throw new BadPacketException(packet.Reason);
    }
}