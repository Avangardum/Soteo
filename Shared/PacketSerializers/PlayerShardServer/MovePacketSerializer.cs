using Soteo.Shared.Packets.PlayerShardServer;
using Soteo.Shared.PacketSerializers.Shared;

namespace Soteo.Shared.PacketSerializers.PlayerShardServer;

public sealed class MovePacketSerializer : PacketSerializer<MovePacket>
{
    protected override int PacketSize(MovePacket packet) => base.PacketSize(packet) + SizeOf(packet.Destination);

    protected override void SerializeInternal(MovePacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeVector2(packet.Destination, ref span);
    }

    protected override MovePacket DeserializeInternal(ref Span<byte> span)
    {
        var message = base.DeserializeInternal(ref span);
        message.Destination = DeserializeVector2(ref span);
        return message;
    }
}