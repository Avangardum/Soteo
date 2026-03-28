using Soteo.Shared.Messages.Master;

namespace Soteo.Shared.MessageSerializers;

public sealed class HandshakeMessageSerializer : MessageSerializer<HandshakeMessage>
{
    protected override int MessageSize(HandshakeMessage message) =>
        base.MessageSize(message) + SizeOf(message.Token) + SizeOf(message.Version);

    protected override void SerializeInternal(HandshakeMessage message, ref Span<byte> span)
    {
        base.SerializeInternal(message, ref span);
        SerializeString(message.Token, ref span);
        SerializeString(message.Version, ref span);
    }

    protected override HandshakeMessage DeserializeInternal(ref Span<byte> span)
    {
        var message = base.DeserializeInternal(ref span);
        message.Token = DeserializeString(ref span);
        message.Version = DeserializeString(ref span);
        return message;
    }
}