namespace Soteo.Core.Networking;

public interface IWebrtcPacketReceiver
{
    void ReceiveWebrtcSdpPacket(WebrtcSdpPacket packet);
    void ReceiveWebrtcIceCandidatePacket(WebrtcIceCandidatePacket packet);
}
