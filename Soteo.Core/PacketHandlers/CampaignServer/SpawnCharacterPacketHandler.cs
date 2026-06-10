using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.CampaignServer.PacketHandlers;

[CampaignServerPacketHandler]
[AllowClientPackets]
public sealed class SpawnCharacterPacketHandler
(
    IUserRepository userRepo,
    IPlayerCharacterRepository charRepo,
    IFromCampaignServerPacketSender packetSender
) : PacketHandler<SpawnCharacterPacket>
{
    protected override void Handle(SpawnCharacterPacket packet, Guid senderId)
    {
        Validate(userRepo.TryGetValue(packet.PeerId, out User? receiver), "Receiver not found");
        User sender = userRepo[senderId];
        Validate
        (
            sender.IsPlayer && receiver.IsShard,
            "Spawn character packet should be sent from a player to a shard server"
        );
        if (!charRepo.TryGetValue(packet.CharacterId, out PlayerCharacter? character))
        {
            character = new PlayerCharacter { Id = packet.CharacterId };
            charRepo.Add(character);
        }
        if (character.ShardId != null) return;
        character.ShardId = packet.PeerId;
        packetSender.RelayFrom(packet, senderId);
    }
}
