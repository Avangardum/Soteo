using Soteo.Shared.Messages.Master;

namespace Soteo.Client.MessageHandlers;

public sealed class WebRtcSdpMessageHandler(IWebRtcSignalingReceiver receiver) : MessageHandler<WebrtcSdpMessage>
{
    protected override void Handle(WebrtcSdpMessage message, Guid senderId)
    {
        receiver.SetRemoteDescription(message);
    }
}