namespace Soteo.Core.Serialization.Packets;

public class PingPacketSerializer(ISerializationHelper s) : PacketSerializer<PingPacket>(s)
{
    protected override void SerializeCore(PingPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.Id, stream);
        s.SerializeBool(packet.IsResponse, stream);
    }

    protected override PingPacket DeserializeCore(Stream stream)
    {
        return new PingPacket
        {
            Id = s.DeserializeGuid(stream),
            IsResponse = s.DeserializeBool(stream)
        };
    }
}
