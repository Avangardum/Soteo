using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.PacketSerializers.Shared;

public sealed class BadInputPacketSerializer : PacketSerializer<BadInputPacket>
{
    protected override int PacketSize(BadInputPacket packet) =>
        base.PacketSize(packet) + SizeOf(packet.Reason);

    protected override void SerializeInternal(BadInputPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeString(packet.Reason, ref span);
    }

    protected override BadInputPacket DeserializeInternal(ref Span<byte> span)
    {
        var packet = base.DeserializeInternal(ref span);
        packet.Reason = DeserializeString(ref span);
        return packet;
    }
}