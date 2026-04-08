using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public sealed class WebRtcIceCandidatePacketHandler(IWebRtcSignalingReceiver receiver) :
    PacketHandler<WebrtcIceCandidatePacket>
{
    protected override void Handle(WebrtcIceCandidatePacket packet, Guid senderId)
    {
        receiver.AddRemoteIceCandidate(packet);
    }
}