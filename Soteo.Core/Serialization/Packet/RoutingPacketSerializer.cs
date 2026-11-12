namespace Soteo.Core.Serialization.Packet;

public sealed class RoutingPacketSerializer
(
    IServiceProvider serviceProvider,
    ISerializationHelper s
) : IPacketSerializer
{
    public Packets.Packet Deserialize(Span<byte> bytes)
    {
        if (bytes.IsEmpty)
            throw new BadPacketException("Packet is empty");
        Type packetType = s.DeserializePacketType(new MemoryStream(bytes.ToArray()));
        IPacketSerializer serializer = serviceProvider.GetPacketSerializerFor(packetType) ??
            throw new BadPacketException($"No serializer exists for packet type {packetType}");
        return serializer.Deserialize(bytes);
    }

    public byte[] Serialize(Packets.Packet packet)
    {
        IPacketSerializer? serializer = serviceProvider.GetPacketSerializerFor(packet.GetType());
        if (serializer == null)
            throw new Exception($"Serializer for {packet.GetType()} not found");
        return serializer.Serialize(packet);
    }
}
