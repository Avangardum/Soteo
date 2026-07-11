using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

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
