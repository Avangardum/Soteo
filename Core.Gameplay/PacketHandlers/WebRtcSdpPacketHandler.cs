using Soteo.CampaignServer.PacketHandlers;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public sealed class WebRtcSdpPacketHandler(IWebrtcPacketReceiver receiver) : PacketHandler<WebrtcSdpPacket>
{
    protected override void Handle(WebrtcSdpPacket packet, Guid senderId)
    {
        receiver.ReceiveWebrtcSdpPacket(packet);
    }
}