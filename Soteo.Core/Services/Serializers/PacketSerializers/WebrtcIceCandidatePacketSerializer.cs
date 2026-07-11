using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class WebrtcIceCandidatePacketSerializer(ISerializationHelper s) :
    PacketSerializer<WebrtcIceCandidatePacket>(s)
{
    protected override void SerializeInternal(WebrtcIceCandidatePacket packet, Stream stream)
    {
        s.SerializeGuid(packet.PeerId, stream);
        s.SerializeString(packet.Media, stream);
        s.SerializeInt(packet.Index, stream);
        s.SerializeString(packet.Name, stream);
    }

    protected override WebrtcIceCandidatePacket DeserializeInternal(Stream stream)
    {
        return new WebrtcIceCandidatePacket
        {
            PeerId = s.DeserializeGuid(stream),
            Media = s.DeserializeString(stream),
            Index = s.DeserializeInt(stream),
            Name = s.DeserializeString(stream),
        };
    }
}
