using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Packets.Master;

namespace Soteo.MasterServer.PacketHandlers;

public sealed class SpawnCharacterPacketHandler
(
    IUserRepository userRepo,
    ICharacterRepository charRepo,
    IPacketSender packetSender
) : PacketHandler<SpawnCharacterPacket>
{
    public override async Task HandleAsync(SpawnCharacterPacket packet, User sender)
    {
        userRepo.TryGetValue(packet.PeerId, out User? receiver);
        Validate(sender.IsPlayer && receiver is { IsShard: true },
            "Spawn character packet should be sent from a player to a shard server");
        if (!charRepo.TryGetValue(sender.Id, out Character? character))
        {
            character = new Character { Id = sender.Id };
            charRepo.Add(character);
        }
        Validate(character.ShardId == Guid.Empty, "Character is already spawned");
        character.ShardId = packet.PeerId;
        await packetSender.RelayFromAsync(packet, sender.Id);
    }
}