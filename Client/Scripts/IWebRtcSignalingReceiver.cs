using Soteo.Shared.Messages.Master;

namespace Soteo.Client;

public interface IWebRtcSignalingReceiver
{
    void SetRemoteDescription(WebrtcSdpMessage message);
    void AddRemoteIceCandidate(WebrtcIceCandidateMessage message);
}