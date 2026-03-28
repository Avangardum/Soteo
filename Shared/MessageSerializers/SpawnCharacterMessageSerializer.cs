using Soteo.Shared.Messages.Master;

namespace Soteo.Shared.MessageSerializers;

public sealed class SpawnCharacterMessageSerializer : RelayedMessageSerializer<SpawnCharacterMessage>
{
    protected override int MessageSize(SpawnCharacterMessage message)
    {
        return base.MessageSize(message) + SizeOf(message.SpawnPointId);
    }

    protected override void SerializeInternal(SpawnCharacterMessage message, ref Span<byte> span)
    {
        base.SerializeInternal(message, ref span);
        SerializeGuid(message.SpawnPointId, ref span);
    }

    protected override SpawnCharacterMessage DeserializeInternal(ref Span<byte> span)
    {
        var message = base.DeserializeInternal(ref span);
        message.SpawnPointId = DeserializeGuid(ref span);
        return message;
    }
}