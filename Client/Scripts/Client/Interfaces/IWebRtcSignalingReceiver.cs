using Soteo.Shared.Packets.MasterServer;

namespace Soteo.Client.Interfaces;

public interface IWebRtcSignalingReceiver
{
    void SetRemoteDescription(WebrtcSdpPacket packet);
    void AddRemoteIceCandidate(WebrtcIceCandidatePacket packet);
}