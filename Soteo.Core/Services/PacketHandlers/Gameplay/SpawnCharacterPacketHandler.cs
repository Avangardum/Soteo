using Soteo.Core.Attributes;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;

[GameplayPacketHandler]
public class SpawnCharacterPacketHandler(IEntityManager entityManager) : PacketHandler<SpawnCharacterPacket>
{
    protected override void Handle(SpawnCharacterPacket packet, Guid senderId)
    {
        entityManager.SpawnPlayerCharacter(packet.CharacterId, packet.PeerId);
    }
}
