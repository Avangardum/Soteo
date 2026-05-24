using Soteo.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Shared.PacketSerializers;

public sealed class WebrtcSdpPacketSerializer : RelayedPacketSerializer<WebrtcSdpPacket>
{
    protected override void SerializeInternal(WebrtcSdpPacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeString(packet.Sdp, stream);
    }

    protected override WebrtcSdpPacket DeserializeInternal(Stream stream)
    {
        WebrtcSdpPacket packet = base.DeserializeInternal(stream);
        packet.Sdp = DeserializeString(stream);
        return packet;
    }
}