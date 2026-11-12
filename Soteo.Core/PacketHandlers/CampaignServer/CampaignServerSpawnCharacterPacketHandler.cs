namespace Soteo.Core.PacketHandlers.CampaignServer;

[CampaignServerPacketHandler]
[AllowClientPackets]
public sealed class CampaignServerSpawnCharacterPacketHandler
(
    IUserRepository userRepo,
    IPlayerCharacterTrackerRepository trackerRepo,
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
        if (!trackerRepo.TryGetValue(packet.CharacterId, out PlayerCharacterTracker? tracker))
        {
            tracker = new PlayerCharacterTracker { Id = packet.CharacterId, Player = sender };
            trackerRepo.Add(tracker);
        }
        if (tracker.Shard != null) return;
        tracker.Shard = receiver;
        packetSender.RelayFrom(packet, senderId);
    }
}
