using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface IWebRtcSignalingReceiver
{
    void SetRemoteDescription(WebrtcSdpPacket packet);
    void AddRemoteIceCandidate(WebrtcIceCandidatePacket packet);
}