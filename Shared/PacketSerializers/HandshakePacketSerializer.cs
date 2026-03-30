using Soteo.Shared.Packets.Master;

namespace Soteo.Shared.PacketSerializers;

public sealed class HandshakePacketSerializer : PacketSerializer<HandshakePacket>
{
    protected override int PacketSize(HandshakePacket packet) =>
        base.PacketSize(packet) + SizeOf(packet.Token) + SizeOf(packet.Version);

    protected override void SerializeInternal(HandshakePacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeString(packet.Token, ref span);
        SerializeString(packet.Version, ref span);
    }

    protected override HandshakePacket DeserializeInternal(ref Span<byte> span)
    {
        var packet = base.DeserializeInternal(ref span);
        packet.Token = DeserializeString(ref span);
        packet.Version = DeserializeString(ref span);
        return packet;
    }
}