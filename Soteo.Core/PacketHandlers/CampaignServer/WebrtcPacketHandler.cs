using Soteo.Core.Attributes;
using Soteo.Core.CampaignServerState.DataObjects;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.CampaignServer;

[CampaignServerPacketHandler]
[AllowClientPackets]
public abstract class WebrtcPacketHandler<TPacket>(IFromCampaignServerPacketSender packetSender, IUserRepository userRepo) :
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