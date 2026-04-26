using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.MasterServer.PacketHandlers;

[AllowClientPackets]
public sealed class SpawnCharacterPacketHandler
(
    IUserRepository userRepo,
    ICharacterRepository charRepo,
    IPacketSender packetSender
) : PacketHandler<SpawnCharacterPacket>
{
    protected override void Handle(SpawnCharacterPacket packet, User sender)
    {
        userRepo.TryGetValue(packet.PeerId, out User? receiver);
        Validate(sender.IsPlayer && receiver is { IsShard: true },
            "Spawn character packet should be sent from a player to a shard server");
        if (!charRepo.TryGetValue(sender.Id, out Character? character))
        {
            character = new Character { Id = sender.Id };
            charRepo.Add(character);
        }
        if (character.ShardId != Guid.Empty) return;
        character.ShardId = packet.PeerId;
        packetSender.RelayFrom(packet, sender.Id);
    }
}