using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class MovePacketSerializer : PacketSerializer<MovePacket>
{
    protected override int PacketSize(MovePacket packet) => base.PacketSize(packet) + SizeOf(packet.Position);

    protected override void SerializeInternal(MovePacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeVector2(packet.Position, ref span);
    }

    protected override MovePacket DeserializeInternal(ref Span<byte> span)
    {
        var message = base.DeserializeInternal(ref span);
        message.Position = DeserializeVector2(ref span);
        return message;
    }
}