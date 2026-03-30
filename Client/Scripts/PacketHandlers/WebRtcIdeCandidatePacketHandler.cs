using Soteo.Shared.Packets.Master;

namespace Soteo.Client.PacketHandlers;

public sealed class WebRtcIdeCandidatePacketHandler(IWebRtcSignalingReceiver receiver) :
    PacketHandler<WebrtcIceCandidatePacket>
{
    protected override void Handle(WebrtcIceCandidatePacket packet, Guid senderId)
    {
        receiver.AddRemoteIceCandidate(packet);
    }
}