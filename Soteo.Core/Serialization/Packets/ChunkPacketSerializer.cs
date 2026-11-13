namespace Soteo.Core.Serialization.Packets;

public sealed class ChunkPacketSerializer(ISerializationHelper s) : PacketSerializer<ChunkPacket>(s)
{
    protected override void SerializeCore(ChunkPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.GroupId, stream);
        s.SerializeInt(packet.Index, stream);
        s.SerializeBool(packet.IsLast, stream);
        s.SerializeList(packet.Bytes, s.SerializeByte, stream);
    }

    protected override ChunkPacket DeserializeCore(Stream stream)
    {
        return new ChunkPacket
        {
            GroupId = s.DeserializeGuid(stream),
            Index = s.DeserializeInt(stream),
            IsLast = s.DeserializeBool(stream),
            Bytes = s.DeserializeList(s.DeserializeByte, stream)
        };
    }
}
