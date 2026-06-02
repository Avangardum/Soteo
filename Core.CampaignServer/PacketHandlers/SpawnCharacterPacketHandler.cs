using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.CampaignServer.PacketHandlers;

[AllowClientPackets]
public sealed class SpawnCharacterPacketHandler
(
    IUserRepository userRepo,
    ICharacterRepository charRepo,
    IPacketSender packetSender
) : PacketHandler<SpawnCharacterPacket>
{
    protected override void Handle(SpawnCharacterPacket packet, Guid senderId)
    {
        Validate(userRepo.TryGetValue(packet.PeerId, out User? receiver), "Receiver not found");
        User sender = userRepo[senderId];
        Validate
        (
            sender.IsPlayer && receiver != null && receiver.IsShard,
            "Spawn character packet should be sent from a player to a shard server"
        );
        if (!charRepo.TryGetValue(senderId, out Character? character))
        {
            character = new Character { Id = senderId };
            charRepo.Add(character);
        }
        if (character.ShardId != null) return;
        character.ShardId = packet.PeerId;
        packetSender.RelayFrom(packet, senderId);
    }
}
