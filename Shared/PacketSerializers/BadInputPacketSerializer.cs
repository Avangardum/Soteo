using Soteo.Shared.Packets.Master;

namespace Soteo.Shared.PacketSerializers;

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