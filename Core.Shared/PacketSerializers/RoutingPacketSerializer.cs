using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Exceptions;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Shared.PacketSerializers;

public sealed class RoutingPacketSerializer : IPacketSerializer
{
    public Packet Deserialize(Span<byte> bytes)
    {
        var type = (PacketType)bytes[0];
        if (!TypeLocator.PacketSerializers.TryGetValue(type, out IPacketSerializer? serializer))
            throw new BadPacketException("Invalid packet type");
        return serializer.Deserialize(bytes);
    }

    public byte[] Serialize(Packet packet)
    {
        if (!TypeLocator.PacketSerializers.TryGetValue(packet.Type, out IPacketSerializer? serializer))
            throw new InvalidOperationException($"Missing serializer for packet type {packet.Type}");
        return serializer.Serialize(packet);
    }
}