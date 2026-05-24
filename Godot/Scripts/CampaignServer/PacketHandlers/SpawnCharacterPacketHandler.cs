using Soteo.CampaignServer.GameState.DataObjects;
using Soteo.CampaignServer.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.CampaignServer.PacketHandlers;

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
        userRepo.TryGetValue(packet.PeerId, out User? receiver);
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
        if (character.ShardId != Guid.Empty) return;
        character.ShardId = packet.PeerId;
        packetSender.RelayFrom(packet, senderId);
    }
}