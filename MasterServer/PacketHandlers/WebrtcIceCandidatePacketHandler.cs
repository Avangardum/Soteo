using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Packets.Master;

namespace Soteo.MasterServer.PacketHandlers;

public sealed class WebrtcIceCandidatePacketHandler(IPacketSender packetSender, IUserRepository userRepo) :
    PacketHandler<WebrtcIceCandidatePacket>
{
    public override async Task HandleAsync(WebrtcIceCandidatePacket packet, User sender)
    {
        if (!userRepo.TryGetValue(packet.PeerId, out User? receiver)) return;
        Validate(sender.IsPlayer && receiver.IsShard || sender.IsShard && receiver.IsPlayer, "WebRTC signaling can only happen between a player and a shard");
        await packetSender.RelayFromAsync(packet, sender.Id);
    }
}