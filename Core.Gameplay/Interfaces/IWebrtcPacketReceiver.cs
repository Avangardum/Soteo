using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface IWebrtcPacketReceiver
{
    void ReceiveWebrtcSdpPacket(WebrtcSdpPacket packet);
    void ReceiveWebrtcIceCandidatePacket(WebrtcIceCandidatePacket packet);
}