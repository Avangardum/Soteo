using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.CampaignServer.PacketHandlers;

[CampaignServerPacketHandler]
[AllowClientPackets]
public abstract class WebrtcPacketHandler<TPacket>(IPacketSender packetSender, IUserRepository userRepo) :
    PacketHandler<TPacket> where TPacket : RelayedPacket
{
    protected override void Handle(TPacket packet, Guid senderId)
    {
        if (!userRepo.TryGetValue(packet.PeerId, out User? receiver)) return;
        User sender = userRepo[senderId];
        Validate
        (
            sender.IsPlayer && receiver.IsShard || sender.IsShard && receiver.IsPlayer,
            "WebRTC signaling can only happen between a player and a shard server"
        );
        packetSender.RelayFrom(packet, senderId);
    }
}