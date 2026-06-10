using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class WebrtcSdpPacketSerializer : PacketSerializer<WebrtcSdpPacket>
{
    protected override void SerializeInternal(WebrtcSdpPacket packet, Stream stream)
    {
        SerializeGuid(packet.PeerId, stream);
        SerializeString(packet.Sdp, stream);
    }

    protected override WebrtcSdpPacket DeserializeInternal(Stream stream)
    {
        return new WebrtcSdpPacket
        {
            PeerId = DeserializeGuid(stream),
            Sdp = DeserializeString(stream),
        };
    }
}
