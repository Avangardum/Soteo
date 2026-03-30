using Soteo.Shared.Packets.MasterServer;
using Soteo.Shared.PacketSerializers.Shared;

namespace Soteo.Shared.PacketSerializers.MasterServer;

public sealed class MasterServerHandshakePacketSerializer : PacketSerializer<MasterServerHandshakePacket>
{
    protected override int PacketSize(MasterServerHandshakePacket packet) =>
        base.PacketSize(packet) + SizeOf(packet.Token) + SizeOf(packet.Version);

    protected override void SerializeInternal(MasterServerHandshakePacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeString(packet.Token, ref span);
        SerializeString(packet.Version, ref span);
    }

    protected override MasterServerHandshakePacket DeserializeInternal(ref Span<byte> span)
    {
        var packet = base.DeserializeInternal(ref span);
        packet.Token = DeserializeString(ref span);
        packet.Version = DeserializeString(ref span);
        return packet;
    }
}