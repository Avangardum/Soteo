using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class ChunkPacketSerializer : PacketSerializer<ChunkPacket>
{
    protected override void SerializeInternal(ChunkPacket packet, Stream stream)
    {
        SerializeGuid(packet.GroupId, stream);
        SerializeInt(packet.Index, stream);
        SerializeBool(packet.IsLast, stream);
        SerializeList(packet.Bytes, SerializeByte, stream);
    }

    protected override ChunkPacket DeserializeInternal(Stream stream)
    {
        return new ChunkPacket
        {
            GroupId = DeserializeGuid(stream),
            Index = DeserializeInt(stream),
            IsLast = DeserializeBool(stream),
            Bytes = DeserializeList(DeserializeByte, stream),
        };
    }
}
