using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.CampaignServer.PacketHandlers;

public sealed class CharacterRecalledPacketHandler(IPlayerCharacterRepository playerCharRepository) :
    PacketHandler<CharacterRecalledPacket>
{
    protected override void Handle(CharacterRecalledPacket packet, Guid senderId)
    {
        playerCharRepository[packet.CharacterId].ShardId = null;
    }
}
