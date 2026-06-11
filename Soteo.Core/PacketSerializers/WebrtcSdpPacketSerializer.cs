using Soteo.Core.Interfaces;
using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class WebrtcSdpPacketSerializer(ISerializationHelper s) : PacketSerializer<WebrtcSdpPacket>(s)
{
    protected override void SerializeInternal(WebrtcSdpPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.PeerId, stream);
        s.SerializeString(packet.Sdp, stream);
    }

    protected override WebrtcSdpPacket DeserializeInternal(Stream stream)
    {
        return new WebrtcSdpPacket
        {
            PeerId = s.DeserializeGuid(stream),
            Sdp = s.DeserializeString(stream),
        };
    }
}
