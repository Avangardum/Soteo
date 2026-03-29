using Soteo.Shared.Messages.Master;

namespace Soteo.Shared.MessageSerializers;

public sealed class BadInputMessageSerializer : MessageSerializer<BadInputMessage>
{
    protected override int MessageSize(BadInputMessage message) =>
        base.MessageSize(message) + SizeOf(message.Reason);

    protected override void SerializeInternal(BadInputMessage message, ref Span<byte> span)
    {
        base.SerializeInternal(message, ref span);
        SerializeString(message.Reason, ref span);
    }

    protected override BadInputMessage DeserializeInternal(ref Span<byte> span)
    {
        var message = base.DeserializeInternal(ref span);
        message.Reason = DeserializeString(ref span);
        return message;
    }
}