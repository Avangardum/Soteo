using Soteo.Core.Attributes;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;

[GameplayPacketHandler]
public sealed class WebRtcSdpPacketHandler(IWebrtcPacketReceiver receiver) : PacketHandler<WebrtcSdpPacket>
{
    protected override void Handle(WebrtcSdpPacket packet, Guid senderId)
    {
        receiver.ReceiveWebrtcSdpPacket(packet);
    }
}
