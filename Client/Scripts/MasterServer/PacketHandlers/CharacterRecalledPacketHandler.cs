using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.MasterServer.PacketHandlers;

public sealed class CharacterRecalledPacketHandler(ICharacterRepository characterRepository) :
    PacketHandler<CharacterRecalledPacket>
{
    protected override void Handle(CharacterRecalledPacket packet, User sender)
    {
        characterRepository[packet.CharacterId].ShardId = Guid.Empty;
    }
}