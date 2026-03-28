using Soteo.Shared.Messages.Master;

namespace Soteo.Shared.MessageSerializers;

public sealed class WebrtcIceCandidateMessageSerializer : RelayedMessageSerializer<WebrtcIceCandidateMessage>
{
    protected override int MessageSize(WebrtcIceCandidateMessage message)
    {
        return base.MessageSize(message) + SizeOf(message.Media) + SizeOf(message.Index) + SizeOf(message.Name);
    }

    protected override void SerializeInternal(WebrtcIceCandidateMessage message, ref Span<byte> span)
    {
        base.SerializeInternal(message, ref span);
        SerializeString(message.Media, ref span);
        SerializeInt(message.Index, ref span);
        SerializeString(message.Name, ref span);
    }

    protected override WebrtcIceCandidateMessage DeserializeInternal(ref Span<byte> span)
    {
        var message = base.DeserializeInternal(ref span);
        message.Media = DeserializeString(ref span);
        message.Index = DeserializeInt(ref span);
        message.Name = DeserializeString(ref span);
        return message;
    }
}