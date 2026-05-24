using Soteo.CampaignServer;
using Soteo.Shared.Enums;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

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