using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public class SpawnCharacterPacketHandler(IEntityManager entityManager, TimeProvider t) : PacketHandler<SpawnCharacterPacket>
{
    public override async Task HandleAsync(SpawnCharacterPacket packet, Guid senderId)
    {
        // todo wait for unpause
        await t.Delay(TimeSpan.FromSeconds(3)); // todo remove
        entityManager.SpawnPlayerCharacter(packet.CharacterId, packet.PeerId);
    }
}
