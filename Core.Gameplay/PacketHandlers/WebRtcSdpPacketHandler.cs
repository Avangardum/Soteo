using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.PacketHandlers;

public sealed class WebRtcSdpPacketHandler(IWebrtcPacketReceiver receiver) : PacketHandler<WebrtcSdpPacket>
{
    protected override void Handle(WebrtcSdpPacket packet, Guid senderId)
    {
        receiver.ReceiveWebrtcSdpPacket(packet);
    }
}
