using Soteo.Core.Dto.Packets;
using Soteo.Core.Enums;
using Soteo.Core.Exceptions;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class RoutingPacketSerializer(IServiceProvider serviceProvider) : IPacketSerializer
{
    public Packet Deserialize(Span<byte> bytes)
    {
        if (bytes.IsEmpty)
            throw new BadPacketException("Packet is empty");
        var packetTypeCode = (PacketTypeCode)bytes[0];
        IPacketSerializer serializer = serviceProvider.GetPacketSerializerFor(packetTypeCode) ??
            throw new BadPacketException($"No serializer exists for packet type code {packetTypeCode}");
        return serializer.Deserialize(bytes);
    }

    public byte[] Serialize(Packet packet)
    {
        IPacketSerializer? serializer = serviceProvider.GetPacketSerializerFor(packet.TypeCode);
        if (serializer == null)
            throw new Exception($"Serializer for {packet.GetType()} not found");
        return serializer.Serialize(packet);
    }
}
