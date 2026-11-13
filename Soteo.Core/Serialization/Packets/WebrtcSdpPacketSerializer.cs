namespace Soteo.Core.Serialization.Packets;

public sealed class WebrtcSdpPacketSerializer(ISerializationHelper s) : PacketSerializer<WebrtcSdpPacket>(s)
{
    protected override void SerializeCore(WebrtcSdpPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.PeerId, stream);
        s.SerializeString(packet.Sdp, stream);
    }

    protected override WebrtcSdpPacket DeserializeCore(Stream stream)
    {
        return new WebrtcSdpPacket
        {
            PeerId = s.DeserializeGuid(stream),
            Sdp = s.DeserializeString(stream)
        };
    }
}
