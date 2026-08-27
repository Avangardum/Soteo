using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public class SpawnCharacterPacketHandler(IEntityManager entityManager) : PacketHandler<SpawnCharacterPacket>
{
    protected override void Handle(SpawnCharacterPacket packet, Guid senderId)
    {
        // It's important to spawn the character immediately, even if the game is paused. Waiting for unpause
        // would risk the situation where a character is stuck in transit (campaign server says it's deployed,
        // but shard server doesn't have it in the entity list), which would cause failure of campaign persistence
        // snapshot creation, since it assumes that such state exists only for a short time.
        entityManager.SpawnPlayerCharacter(packet.CharacterId, packet.PeerId);
    }
}
