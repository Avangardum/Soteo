using Soteo.Client.Interfaces;
using Soteo.Shared.Packets.MasterServer;

namespace Soteo.Client.PacketHandlers;

public class SpawnCharacterPacketHandler(IEntitySpawner spawner) : PacketHandler<SpawnCharacterPacket>
{
    protected override void Handle(SpawnCharacterPacket packet, Guid senderId)
    {
        ValidateThisIsServer();
        ValidateIsMasterServer(senderId);
        spawner.SpawnPlayerCharacter(packet.PeerId);
    }
}