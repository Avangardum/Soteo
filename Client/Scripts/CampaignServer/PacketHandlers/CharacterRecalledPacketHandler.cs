using Soteo.CampaignServer.GameState.DataObjects;
using Soteo.CampaignServer.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.CampaignServer.PacketHandlers;

public sealed class CharacterRecalledPacketHandler(ICharacterRepository characterRepository) :
    PacketHandler<CharacterRecalledPacket>
{
    protected override void Handle(CharacterRecalledPacket packet, User sender)
    {
        characterRepository[packet.CharacterId].ShardId = Guid.Empty;
    }
}