using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;
using Soteo.Core.Models;

namespace Soteo.Core.Services.PacketHandlers.CampaignServer;

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
        if (!charRepo.TryGetValue(packet.CharacterId, out PlayerCharacterTracker? character))
        {
            character = new PlayerCharacterTracker { Id = packet.CharacterId };
            charRepo.Add(character);
        }
        if (character.ShardId != null) return;
        character.ShardId = packet.PeerId;
        packetSender.RelayFrom(packet, senderId);
    }
}
