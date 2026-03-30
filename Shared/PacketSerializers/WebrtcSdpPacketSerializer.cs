using Soteo.Shared.Packets.Master;

namespace Soteo.Shared.PacketSerializers;

public sealed class WebrtcSdpPacketSerializer : RelayedPacketSerializer<WebrtcSdpPacket>
{
    protected override int PacketSize(WebrtcSdpPacket packet) =>
        base.PacketSize(packet) + SizeOf(packet.SdpType) + SizeOf(packet.Sdp);

    protected override void SerializeInternal(WebrtcSdpPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeString(packet.SdpType, ref span);
        SerializeString(packet.Sdp, ref span);
    }

    protected override WebrtcSdpPacket DeserializeInternal(ref Span<byte> span)
    {
        WebrtcSdpPacket packet = base.DeserializeInternal(ref span);
        packet.SdpType = DeserializeString(ref span);
        packet.Sdp = DeserializeString(ref span);
        return packet;
    }
}