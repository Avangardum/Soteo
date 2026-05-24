using Soteo.CampaignServer.PacketHandlers;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public class BadInputPacketHandler : PacketHandler<BadInputPacket>
{
    protected override void Handle(BadInputPacket packet, Guid senderId)
    {
        throw new BadPacketException(packet.Reason);
    }
}