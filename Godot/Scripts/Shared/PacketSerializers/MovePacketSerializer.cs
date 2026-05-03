using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class MovePacketSerializer : PacketSerializer<MovePacket>
{
    protected override void SerializeInternal(MovePacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeVector2(packet.Position, stream);
    }

    protected override MovePacket DeserializeInternal(Stream stream)
    {
        var message = base.DeserializeInternal(stream);
        message.Position = DeserializeVector2(stream);
        return message;
    }
}