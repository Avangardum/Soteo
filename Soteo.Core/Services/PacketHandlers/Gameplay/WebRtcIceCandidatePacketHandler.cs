using Soteo.Core.Attributes;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;

[GameplayPacketHandler]
public sealed class WebRtcIceCandidatePacketHandler(IWebrtcPacketReceiver receiver) :
    PacketHandler<WebrtcIceCandidatePacket>
{
    protected override void Handle(WebrtcIceCandidatePacket packet, Guid senderId)
    {
        receiver.ReceiveWebrtcIceCandidatePacket(packet);
    }
}
