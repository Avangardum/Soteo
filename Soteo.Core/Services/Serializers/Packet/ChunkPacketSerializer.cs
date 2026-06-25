using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

public sealed class ChunkPacketSerializer(ISerializationHelper s) : PacketSerializer<ChunkPacket>(s)
{
    protected override void SerializeInternal(ChunkPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.GroupId, stream);
        s.SerializeInt(packet.Index, stream);
        s.SerializeBool(packet.IsLast, stream);
        s.SerializeList(packet.Bytes, s.SerializeByte, stream);
    }

    protected override ChunkPacket DeserializeInternal(Stream stream)
    {
        return new ChunkPacket
        {
            GroupId = s.DeserializeGuid(stream),
            Index = s.DeserializeInt(stream),
            IsLast = s.DeserializeBool(stream),
            Bytes = s.DeserializeList(s.DeserializeByte, stream),
        };
    }
}
