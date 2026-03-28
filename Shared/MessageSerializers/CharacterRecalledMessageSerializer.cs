using Soteo.Shared.Messages.Master;

namespace Soteo.Shared.MessageSerializers;

public sealed class CharacterRecalledMessageSerializer : MessageSerializer<CharacterRecalledMessage>
{
    protected override int MessageSize(CharacterRecalledMessage message)
    {
        return base.MessageSize(message) + SizeOf(message.CharacterId);
    }

    protected override void SerializeInternal(CharacterRecalledMessage message, ref Span<byte> span)
    {
        base.SerializeInternal(message, ref span);
        SerializeGuid(message.CharacterId, ref span);
    }

    protected override CharacterRecalledMessage DeserializeInternal(ref Span<byte> span)
    {
        var message = base.DeserializeInternal(ref span);
        message.CharacterId = DeserializeGuid(ref span);
        return message;
    }
}