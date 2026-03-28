using Soteo.Shared.Messages.Master;

namespace Soteo.Shared.MessageSerializers;

public abstract class RelayedMessageSerializer<TMessage> : MessageSerializer<TMessage>
    where TMessage : RelayedMessage, new()
{
    protected override int MessageSize(TMessage message) => base.MessageSize(message) + Const.BytesInGuid;

    protected override void SerializeInternal(TMessage message, ref Span<byte> span)
    {
        base.SerializeInternal(message, ref span);
        SerializeGuid(message.PeerId, ref span);
    }

    protected override TMessage DeserializeInternal(ref Span<byte> span)
    {
        TMessage message = base.DeserializeInternal(ref span);
        message.PeerId = DeserializeGuid(ref span);
        return message;
    }
}