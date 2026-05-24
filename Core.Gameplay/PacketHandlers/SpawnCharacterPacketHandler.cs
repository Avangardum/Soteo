using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.PacketHandlers;

public class SpawnCharacterPacketHandler(IEntityManager entityManager) : PacketHandler<SpawnCharacterPacket>
{
    protected override void Handle(SpawnCharacterPacket packet, Guid senderId)
    {
        entityManager.SpawnPlayerCharacter(packet.PeerId);
    }
}