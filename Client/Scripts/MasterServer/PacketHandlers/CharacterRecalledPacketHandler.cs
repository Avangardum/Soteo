using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Packets.MasterServer;

namespace Soteo.MasterServer.PacketHandlers;

public sealed class CharacterRecalledPacketHandler(ICharacterRepository characterRepository) :
    PacketHandler<CharacterRecalledPacket>
{
    protected override void Handle(CharacterRecalledPacket packet, User sender)
    {
        characterRepository.TryGetValue(packet.CharacterId, out Character? character);
        Validate(sender.IsShard, "Only shards can recall characters");
        Validate(character?.ShardId == sender.Id, "Character is not in this shard");
        character.ShardId = Guid.Empty;
    }
}