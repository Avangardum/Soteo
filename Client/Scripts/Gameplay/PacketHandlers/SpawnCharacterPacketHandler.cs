using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public class SpawnCharacterPacketHandler(IEntitySpawner spawner) : PacketHandler<SpawnCharacterPacket>
{
    protected override void Handle(SpawnCharacterPacket packet, Guid senderId)
    {
        ValidateThisIsServer();
        ValidateIsMasterServer(senderId);
        spawner.SpawnPlayerCharacter(packet.PeerId);
    }
}