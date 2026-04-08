using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class WebrtcIceCandidatePacketSerializer : RelayedPacketSerializer<WebrtcIceCandidatePacket>
{
    protected override int PacketSize(WebrtcIceCandidatePacket packet)
    {
        return base.PacketSize(packet) + SizeOf(packet.Media) + SizeOf(packet.Index) + SizeOf(packet.Name);
    }

    protected override void SerializeInternal(WebrtcIceCandidatePacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeString(packet.Media, ref span);
        SerializeInt(packet.Index, ref span);
        SerializeString(packet.Name, ref span);
    }

    protected override WebrtcIceCandidatePacket DeserializeInternal(ref Span<byte> span)
    {
        var packet = base.DeserializeInternal(ref span);
        packet.Media = DeserializeString(ref span);
        packet.Index = DeserializeInt(ref span);
        packet.Name = DeserializeString(ref span);
        return packet;
    }
}