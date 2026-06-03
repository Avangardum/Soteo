using Soteo.Core.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Shared.PacketSerializers;

public sealed class WebrtcSdpPacketSerializer : PacketSerializer<WebrtcSdpPacket>
{
    protected override void SerializeInternal(WebrtcSdpPacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
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
