using Soteo.Shared.Messages.Master;

namespace Soteo.Shared.MessageSerializers;

public sealed class WebrtcSdpMessageSerializer : RelayedMessageSerializer<WebrtcSdpMessage>
{
    protected override int MessageSize(WebrtcSdpMessage message) =>
        base.MessageSize(message) + SizeOf(message.SdpType) + SizeOf(message.Sdp);

    protected override void SerializeInternal(WebrtcSdpMessage message, ref Span<byte> span)
    {
        base.SerializeInternal(message, ref span);
        SerializeString(message.SdpType, ref span);
        SerializeString(message.Sdp, ref span);
    }

    protected override WebrtcSdpMessage DeserializeInternal(ref Span<byte> span)
    {
        WebrtcSdpMessage message = base.DeserializeInternal(ref span);
        message.SdpType = DeserializeString(ref span);
        message.Sdp = DeserializeString(ref span);
        return message;
    }
}