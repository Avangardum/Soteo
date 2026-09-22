using System.Collections.Immutable;
using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Enums;
using Soteo.Core.Exceptions;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public static class PacketSerializer
{
    private static ImmutableDictionary<Type, Type>? _typesByPacketType;

    public static Type? TypeFor(Type packetType, ITypeLocator typeLocator)
    {
        _typesByPacketType ??= InitTypesByPacketType(typeLocator);
        return _typesByPacketType.GetOrDefault(packetType);
    }

    public static IReadOnlyList<Type> AllTypes(ITypeLocator typeLocator)
    {
        _typesByPacketType ??= InitTypesByPacketType(typeLocator);
        return _typesByPacketType.Values.ToImmutableList();
    }

    private static ImmutableDictionary<Type, Type> InitTypesByPacketType(ITypeLocator typeLocator)
    {
        IReadOnlyList<Type> locatedSerializerTypes = typeLocator.ConcreteSubclassesOf<IPacketSerializer>();
        return typeLocator
            .ConcreteSubclassesOf<Packet>()
            .ToImmutableDictionary(it => it, it => InitTypeFor(it, locatedSerializerTypes));
    }

    private static Type InitTypeFor(Type packetType, IReadOnlyList<Type> locatedSerializerTypes)
    {
        Type? typeFromPacketSerializerAttribute =
            packetType.GetCustomAttribute<PacketSerializerAttribute>()?.SerializerType;
        Type? typeFromEmptyPacketAttribute = packetType.HasAttribute<EmptyPacketAttribute>() ?
            typeof(EmptyPacketSerializer<>).MakeGenericType(packetType) :
            null;
        IReadOnlyList<Type> typesFromLocator = locatedSerializerTypes
            .Where(it => it.SingleTypeArgOfGenericDefinitionOrNull(typeof(PacketSerializer<>)) == packetType)
            .ToImmutableList();
        if (typesFromLocator.Count > 1)
            throw new Exception($"Found multiple classes inheriting from PacketSerializer<{packetType}>");
        Type? typeFromLocator = typesFromLocator.SingleOrDefault();

        ImmutableList<Type> types =
            ImmutableList.Create(typeFromPacketSerializerAttribute, typeFromEmptyPacketAttribute, typeFromLocator)
            .WhereNotNull()
            .ToImmutableList();
        if (types.Count == 1) return types[0];

        var x1 = typeFromPacketSerializerAttribute != null ? "x" : " ";
        var x2 = typeFromEmptyPacketAttribute != null ? "x" : " ";
        var x3 = typeFromLocator != null ? "x" : " ";
        var message =
            $"""
             Failed to find a serializer for {packetType}. Exactly one of the following conditions should be fulfilled:
             [{x1}] Packet is decorated with [PacketSerializer(...)]
             [{x2}] Packet is decorated with [EmptyPacket]
             [{x3}] Serializer inheriting from PacketSerializer<{packetType}> exists and visible to TypeLocator
             """;
        throw new Exception(message);
    }
}

public abstract class PacketSerializer<TPacket>(ISerializationHelper s) : IPacketSerializer where TPacket : Packet
{
    byte[] IPacketSerializer.Serialize(Packet packet) => Serialize((TPacket)packet);

    public byte[] Serialize(TPacket packet)
    {
        var stream = new MemoryStream();
        s.SerializePacketType(packet.GetType(), stream);
        SerializeInternal(packet, stream);
        return stream.ToArray();
    }

    protected abstract void SerializeInternal(TPacket packet, Stream stream);

    Packet IPacketSerializer.Deserialize(Span<byte> bytes) => Deserialize(bytes);

    public TPacket Deserialize(Span<byte> bytes)
    {
        try
        {
            var stream = new MemoryStream(bytes.ToArray());
            Type type = s.DeserializePacketType(stream);
            if (type != typeof(TPacket))
                throw new InvalidOperationException("Wrong serializer");
            TPacket packet = DeserializeInternal(stream);
            if (stream.Position != bytes.Length)
            {
                throw new BadSerializedDataException
                (
                    $"Packet deserialized as {packet}, but contains {bytes.Length - stream.Position} extra bytes"
                );
            }
            return packet;
        }
        catch (BadSerializedDataException e)
        {
            throw new BadSerializedDataException($"Bad packet\n{BitConverter.ToString(bytes.ToArray())}\n", e);
        }
    }

    protected abstract TPacket DeserializeInternal(Stream stream);
}
