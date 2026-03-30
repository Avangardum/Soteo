using Soteo.Shared.Packets.Master;

namespace Soteo.Client;

public interface IWebRtcSignalingReceiver
{
    void SetRemoteDescription(WebrtcSdpPacket packet);
    void AddRemoteIceCandidate(WebrtcIceCandidatePacket packet);
}