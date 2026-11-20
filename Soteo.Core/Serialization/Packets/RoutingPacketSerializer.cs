namespace Soteo.Core.Serialization.Packets;

public sealed class RoutingPacketSerializer
(
    IServiceProvider serviceProvider,
    SerializationHelper s
) : IPacketSerializer
{
    public Packet Deserialize(byte[] bytes)
    {
        if (bytes.Length == 0)
            throw new BadPacketException("Packet is empty");
        Type packetType = s.DeserializePacketType(new MemoryStream(bytes));
        IPacketSerializer serializer = serviceProvider.GetPacketSerializerFor(packetType) ??
            throw new BadPacketException($"No serializer exists for packet type {packetType}");
        return serializer.Deserialize(bytes);
    }

    public byte[] Serialize(Packet packet)
    {
        IPacketSerializer? serializer = serviceProvider.GetPacketSerializerFor(packet.GetType());
        if (serializer == null)
            throw new Exception($"Serializer for {packet.GetType()} not found");
        return serializer.Serialize(packet);
    }
}
