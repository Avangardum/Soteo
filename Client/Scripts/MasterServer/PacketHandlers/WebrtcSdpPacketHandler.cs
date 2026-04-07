using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Packets.MasterServer;

namespace Soteo.MasterServer.PacketHandlers;

public sealed class WebrtcSdpPacketHandler(IPacketSender packetSender, IUserRepository userRepo) :
    PacketHandler<WebrtcSdpPacket>
{
    public override async Task HandleAsync(WebrtcSdpPacket packet, User sender)
    {
        if (!userRepo.TryGetValue(packet.PeerId, out User? receiver)) return;
        Validate(sender.IsPlayer && receiver.IsShard || sender.IsShard && receiver.IsPlayer,
            "WebRTC signaling can only happen between a player and a shard");
        packetSender.RelayFrom(packet, sender.Id);
    }
}