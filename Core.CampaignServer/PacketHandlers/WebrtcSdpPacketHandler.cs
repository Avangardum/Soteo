using Soteo.CampaignServer.GameState.DataObjects;
using Soteo.CampaignServer.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.CampaignServer.PacketHandlers;

[AllowClientPackets]
public sealed class WebrtcSdpPacketHandler(IPacketSender packetSender, IUserRepository userRepo) :
    PacketHandler<WebrtcSdpPacket>
{
    protected override void Handle(WebrtcSdpPacket packet, Guid senderId)
    {
        if (!userRepo.TryGetValue(packet.PeerId, out User? receiver)) return;
        User sender = userRepo[senderId];
        Validate(sender.IsPlayer && receiver.IsShard || sender.IsShard && receiver.IsPlayer,
            "WebRTC signaling can only happen between a player and a shard server");
        packetSender.RelayFrom(packet, senderId);
    }
}