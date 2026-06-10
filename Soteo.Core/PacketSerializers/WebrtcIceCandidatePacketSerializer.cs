using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class WebrtcIceCandidatePacketSerializer : PacketSerializer<WebrtcIceCandidatePacket>
{
    protected override void SerializeInternal(WebrtcIceCandidatePacket packet, Stream stream)
    {
        SerializeGuid(packet.PeerId, stream);
        SerializeString(packet.Media, stream);
        SerializeInt(packet.Index, stream);
        SerializeString(packet.Name, stream);
    }

    protected override WebrtcIceCandidatePacket DeserializeInternal(Stream stream)
    {
        return new WebrtcIceCandidatePacket
        {
            PeerId = DeserializeGuid(stream),
            Media = DeserializeString(stream),
            Index = DeserializeInt(stream),
            Name = DeserializeString(stream),
        };
    }
}
