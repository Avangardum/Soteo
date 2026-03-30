using Soteo.Shared.Packets.Master;

namespace Soteo.Client.PacketHandlers;

public class SpawnCharacterPacketHandler(ICharacterSpawner spawner) : PacketHandler<SpawnCharacterPacket>
{
    protected override void Handle(SpawnCharacterPacket packet, Guid senderId)
    {
        ValidateThisIsServer();
        ValidateIsMasterServer(senderId);
        spawner.SpawnPlayerCharacter(packet.PeerId);
    }
}