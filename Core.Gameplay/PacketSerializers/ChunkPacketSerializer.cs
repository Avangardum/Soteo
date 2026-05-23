using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class ChunkPacketSerializer : PacketSerializer<ChunkPacket>
{
    protected override void SerializeInternal(ChunkPacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeGuid(packet.GroupId, stream);
        SerializeInt(packet.Index, stream);
        SerializeBool(packet.IsLast, stream);
        SerializeList(packet.Bytes, SerializeByte, stream);
    }

    protected override ChunkPacket DeserializeInternal(Stream stream)
    {
        var packet = base.DeserializeInternal(stream);
        packet.GroupId = DeserializeGuid(stream);
        packet.Index = DeserializeInt(stream);
        packet.IsLast = DeserializeBool(stream);
        packet.Bytes = DeserializeList(DeserializeByte, stream);
        return packet;
    }
}