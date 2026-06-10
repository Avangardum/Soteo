using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Exceptions;
using Soteo.Core.Shared.Extensions;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Shared.PacketSerializers;

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

    public byte[] Serialize(Packet packet) =>
        serviceProvider.GetPacketSerializerFor(packet.TypeCode).Required.Serialize(packet);
}
