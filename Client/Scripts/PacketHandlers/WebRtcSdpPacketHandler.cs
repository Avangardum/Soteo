using Soteo.Client.Interfaces;
using Soteo.Shared.Packets.MasterServer;

namespace Soteo.Client.PacketHandlers;

public sealed class WebRtcSdpPacketHandler(IWebRtcSignalingReceiver receiver) : PacketHandler<WebrtcSdpPacket>
{
    protected override void Handle(WebrtcSdpPacket packet, Guid senderId)
    {
        receiver.SetRemoteDescription(packet);
    }
}