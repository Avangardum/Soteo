using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public class PingPacketSerializer : PacketSerializer<PingPacket>
{
    protected override int PacketSize(PingPacket packet) =>
        base.PacketSize(packet) + SizeOf(packet.Id) + SizeOf(packet.IsResponse);

    protected override void SerializeInternal(PingPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeGuid(packet.Id, ref span);
        SerializeBool(packet.IsResponse, ref span);
    }

    protected override PingPacket DeserializeInternal(ref Span<byte> span)
    {
        var packet = base.DeserializeInternal(ref span);
        packet.Id = DeserializeGuid(ref span);
        packet.IsResponse = DeserializeBool(ref span);
        return packet;
    }
}