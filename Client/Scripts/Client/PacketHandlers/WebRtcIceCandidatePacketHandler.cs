using Soteo.Client.Interfaces;
using Soteo.Shared.Packets.MasterServer;

namespace Soteo.Client.PacketHandlers;

public sealed class WebRtcIceCandidatePacketHandler(IWebRtcSignalingReceiver receiver) :
    PacketHandler<WebrtcIceCandidatePacket>
{
    protected override void Handle(WebrtcIceCandidatePacket packet, Guid senderId)
    {
        receiver.AddRemoteIceCandidate(packet);
    }
}