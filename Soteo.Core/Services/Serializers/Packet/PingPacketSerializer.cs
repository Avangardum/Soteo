using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

public class PingPacketSerializer(ISerializationHelper s) : PacketSerializer<PingPacket>(s)
{
    protected override void SerializeInternal(PingPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.Id, stream);
        s.SerializeBool(packet.IsResponse, stream);
    }

    protected override PingPacket DeserializeInternal(Stream stream)
    {
        return new PingPacket
        {
            Id = s.DeserializeGuid(stream),
            IsResponse = s.DeserializeBool(stream),
        };
    }
}
