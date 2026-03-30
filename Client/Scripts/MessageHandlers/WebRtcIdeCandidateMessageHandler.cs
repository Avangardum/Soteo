using Soteo.Shared.Messages.Master;

namespace Soteo.Client.MessageHandlers;

public sealed class WebRtcIdeCandidateMessageHandler(IWebRtcSignalingReceiver receiver) :
    MessageHandler<WebrtcIceCandidateMessage>
{
    protected override void Handle(WebrtcIceCandidateMessage message, Guid senderId)
    {
        receiver.AddRemoteIceCandidate(message);
    }
}