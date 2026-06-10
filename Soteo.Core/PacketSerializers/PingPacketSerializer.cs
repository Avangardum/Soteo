using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public class PingPacketSerializer : PacketSerializer<PingPacket>
{
    protected override void SerializeInternal(PingPacket packet, Stream stream)
    {
        SerializeGuid(packet.Id, stream);
        SerializeBool(packet.IsResponse, stream);
    }

    protected override PingPacket DeserializeInternal(Stream stream)
    {
        return new PingPacket
        {
            Id = DeserializeGuid(stream),
            IsResponse = DeserializeBool(stream),
        };
    }
}
