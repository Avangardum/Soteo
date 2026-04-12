using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.MasterServer.PacketHandlers;

[AllowClientPackets]
public sealed class WebrtcIceCandidatePacketHandler(IPacketSender packetSender, IUserRepository userRepo) :
    PacketHandler<WebrtcIceCandidatePacket>
{
    protected override void Handle(WebrtcIceCandidatePacket packet, User sender)
    {
        if (!userRepo.TryGetValue(packet.PeerId, out User? receiver)) return;
        Validate(sender.IsPlayer && receiver.IsShard || sender.IsShard && receiver.IsPlayer,
            "WebRTC signaling can only happen between a player and a shard server");
        packetSender.RelayFrom(packet, sender.Id);
    }
}