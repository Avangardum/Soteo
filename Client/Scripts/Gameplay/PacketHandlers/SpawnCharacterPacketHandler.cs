using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public class SpawnCharacterPacketHandler(IEntityManager entityManager) : PacketHandler<SpawnCharacterPacket>
{
    protected override void Handle(SpawnCharacterPacket packet, Guid senderId)
    {
        ValidateThisIsServer();
        ValidateIsMasterServer(senderId);
        entityManager.SpawnPlayerCharacter(packet.PeerId);
    }
}