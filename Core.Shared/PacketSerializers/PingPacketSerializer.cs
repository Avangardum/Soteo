using Soteo.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Shared.PacketSerializers;

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
        var packet = base.DeserializeInternal(stream);
        packet.Id = DeserializeGuid(stream);
        packet.IsResponse = DeserializeBool(stream);
        return packet;
    }
}