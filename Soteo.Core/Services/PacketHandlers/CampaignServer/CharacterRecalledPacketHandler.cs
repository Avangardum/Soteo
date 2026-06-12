using Soteo.Core.Attributes;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.CampaignServer;

[CampaignServerPacketHandler]
public sealed class CharacterRecalledPacketHandler(IPlayerCharacterRepository playerCharRepository) :
    PacketHandler<CharacterRecalledPacket>
{
    protected override void Handle(CharacterRecalledPacket packet, Guid senderId)
    {
        playerCharRepository[packet.CharacterId].ShardId = null;
    }
}
