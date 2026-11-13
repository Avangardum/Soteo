namespace Soteo.Core.Serialization.Packets;

public sealed class WebrtcIceCandidatePacketSerializer(ISerializationHelper s) :
    PacketSerializer<WebrtcIceCandidatePacket>(s)
{
    protected override void SerializeCore(WebrtcIceCandidatePacket packet, Stream stream)
    {
        s.SerializeGuid(packet.PeerId, stream);
        s.SerializeString(packet.Media, stream);
        s.SerializeInt(packet.Index, stream);
        s.SerializeString(packet.Name, stream);
    }

    protected override WebrtcIceCandidatePacket DeserializeCore(Stream stream)
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
