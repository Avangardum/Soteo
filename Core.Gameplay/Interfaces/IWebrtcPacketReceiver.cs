using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Interfaces;

public interface IWebrtcPacketReceiver
{
    void ReceiveWebrtcSdpPacket(WebrtcSdpPacket packet);
    void ReceiveWebrtcIceCandidatePacket(WebrtcIceCandidatePacket packet);
}
