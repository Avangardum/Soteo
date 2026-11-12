using Soteo.Core.Dto.Packets;

namespace Soteo.Core.Interfaces;

public interface IWebrtcPacketReceiver
{
    void ReceiveWebrtcSdpPacket(WebrtcSdpPacket packet);
    void ReceiveWebrtcIceCandidatePacket(WebrtcIceCandidatePacket packet);
}
