using Soteo.Core.Enums;
using Soteo.Core.Exceptions;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

public sealed class RoutingPacketSerializer(IServiceProvider serviceProvider) : IPacketSerializer
{
    public Dto.Packets.Packet Deserialize(Span<byte> bytes)
    {
        if (bytes.IsEmpty)
            throw new BadPacketException("Packet is empty");
        var packetTypeCode = (PacketTypeCode)bytes[0];
        IPacketSerializer serializer = serviceProvider.GetPacketSerializerFor(packetTypeCode) ??
            throw new BadPacketException($"No serializer exists for packet type code {packetTypeCode}");
        return serializer.Deserialize(bytes);
    }

    public byte[] Serialize(Dto.Packets.Packet packet) =>
        serviceProvider.GetPacketSerializerFor(packet.TypeCode).Required.Serialize(packet);
}
