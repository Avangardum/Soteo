namespace Soteo.Core.PacketHandlers.CampaignServer;

[CampaignServerPacketHandler]
public sealed class CharacterRecalledPacketHandler(IPlayerCharacterTrackerRepository playerCharTrackerRepository) :
    PacketHandler<CharacterRecalledPacket>
{
    protected override void Handle(CharacterRecalledPacket packet, Guid senderId)
    {
        playerCharTrackerRepository[packet.CharacterId].Shard = null;
    }
}
