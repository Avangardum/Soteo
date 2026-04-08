using Soteo.Shared.Enums;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.PacketSerializers.Shared;

public sealed class RoutingPacketSerializer : IPacketSerializer
{
    private readonly Dictionary<PacketType, IPacketSerializer> _serializers;

    public RoutingPacketSerializer()
    {
        _serializers = typeof(IPacketSerializer).Assembly.ExportedTypes
            .Where(it =>
                !it.IsAbstract &&
                it != typeof(RoutingPacketSerializer) &&
                it.IsAssignableTo(typeof(IPacketSerializer)))
            .ToDictionary
            (
                it => it.GetPacketType(typeof(PacketSerializer<>)),
                it => (IPacketSerializer)Activator.CreateInstance(it)!
            );
    }
    
    public Packet Deserialize(Span<byte> bytes)
    {
        var type = (PacketType)bytes[0];
        if (!_serializers.TryGetValue(type, out IPacketSerializer serializer))
            throw new BadPacketException("Invalid packet type");
        return serializer.Deserialize(bytes);
    }

    public byte[] Serialize(Packet packet)
    {
        if (!_serializers.TryGetValue(packet.Type, out IPacketSerializer serializer))
            throw new InvalidOperationException($"Missing serializer for packet type {packet.Type}");
        return serializer.Serialize(packet);
    }
}