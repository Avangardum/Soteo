using Soteo.Core.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Shared.PacketSerializers;

public class PingPacketSerializer : PacketSerializer<PingPacket>
{
    protected override void SerializeInternal(PingPacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
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
