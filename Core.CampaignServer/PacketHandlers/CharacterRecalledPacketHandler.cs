using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.CampaignServer.PacketHandlers;

public sealed class CharacterRecalledPacketHandler(ICharacterRepository characterRepository) :
    PacketHandler<CharacterRecalledPacket>
{
    protected override void Handle(CharacterRecalledPacket packet, Guid senderId)
    {
        characterRepository[packet.CharacterId].ShardId = Guid.Empty;
    }
}