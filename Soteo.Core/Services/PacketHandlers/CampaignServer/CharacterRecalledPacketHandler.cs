using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.CampaignServer;

[CampaignServerPacketHandler]
public sealed class CharacterRecalledPacketHandler(IPlayerCharacterTrackerRepository playerCharTrackerRepository) :
    PacketHandler<CharacterRecalledPacket>
{
    protected override void Handle(CharacterRecalledPacket packet, Guid senderId)
    {
        playerCharTrackerRepository[packet.CharacterId].ShardId = null;
    }
}
