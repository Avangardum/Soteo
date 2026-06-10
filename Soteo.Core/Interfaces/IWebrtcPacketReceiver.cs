using Soteo.Core.Packets;

namespace Soteo.Core.Interfaces;

public interface IWebrtcPacketReceiver
{
    void ReceiveWebrtcSdpPacket(WebrtcSdpPacket packet);
    void ReceiveWebrtcIceCandidatePacket(WebrtcIceCandidatePacket packet);
}
