using Soteo.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Shared.PacketSerializers;

public sealed class WebrtcIceCandidatePacketSerializer : RelayedPacketSerializer<WebrtcIceCandidatePacket>
{
    protected override void SerializeInternal(WebrtcIceCandidatePacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeString(packet.Media, stream);
        SerializeInt(packet.Index, stream);
        SerializeString(packet.Name, stream);
    }

    protected override WebrtcIceCandidatePacket DeserializeInternal(Stream stream)
    {
        var packet = base.DeserializeInternal(stream);
        packet.Media = DeserializeString(stream);
        packet.Index = DeserializeInt(stream);
        packet.Name = DeserializeString(stream);
        return packet;
    }
}