using System.Reflection;

namespace Soteo.Core.Serialization.Packets;

public static class PacketSerializer
{
    private static IReadOnlyDictionary<Type, Type>? _typesByPacketType;

    public static Type? TypeFor(Type packetType, ITypeLocator typeLocator)
    {
        _typesByPacketType ??= InitTypesByPacketType(typeLocator);
        return _typesByPacketType.GetValueOrDefault(packetType);
    }

    public static IReadOnlyList<Type> AllTypes(ITypeLocator typeLocator)
    {
        _typesByPacketType ??= InitTypesByPacketType(typeLocator);
        return _typesByPacketType.Values.ToList().AsReadOnly();
    }

    private static IReadOnlyDictionary<Type, Type> InitTypesByPacketType(ITypeLocator typeLocator)
    {
        IReadOnlyList<Type> locatedSerializerTypes = typeLocator.ConcreteSubclassesOf<IPacketSerializer>();
        return typeLocator
            .ConcreteSubclassesOf<Packet>()
            .ToDictionary(it => it, it => InitTypeFor(it, locatedSerializerTypes)).AsReadOnly();
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
            .ToList().AsReadOnly();
        if (typesFromLocator.Count > 1)
            throw new Exception($"Found multiple classes inheriting from PacketSerializer<{packetType}>");
        Type? typeFromLocator = typesFromLocator.SingleOrDefault();

        IReadOnlyList<Type> types =
            new[] { typeFromPacketSerializerAttribute, typeFromEmptyPacketAttribute, typeFromLocator }
            .WhereNotNull()
            .ToList().AsReadOnly();
        if (types.Count == 1) return types[0];

        var x1 = typeFromPacketSerializerAttribute != null ? "x" : " ";
        var x2 = typeFromEmptyPacketAttribute != null ? "x" : " ";
        var x3 = typeFromLocator != null ? "x" : " ";
        var message =
            $"""
             Failed to find a serializer for {packetType}. Exactly one of the following conditions should be fulfilled:
             [{x1}] Packet is decorated with [PacketSerializer(...)]
             [{x2}] Packet is decorated with [EmptyPacket]
             [{x3}] Serializer inheriting from PacketSerializer<{packetType}> exists and is visible to TypeLocator
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
        SerializeCore(packet, stream);
        return stream.ToArray();
    }

    protected abstract void SerializeCore(TPacket packet, Stream stream);

    Packet IPacketSerializer.Deserialize(Span<byte> bytes) => Deserialize(bytes);

    public TPacket Deserialize(Span<byte> bytes)
    {
        try
        {
            var stream = new MemoryStream(bytes.ToArray());
            Type type = s.DeserializePacketType(stream);
            if (type != typeof(TPacket))
                throw new InvalidOperationException("Wrong serializer");
            TPacket packet = DeserializeCore(stream);
            if (stream.Position != bytes.Length)
            {
                throw new BadSerializedDataException
                (
                    $"Packet deserialized as {packet}, but contains {bytes.Length - stream.Position} extra bytes"
                );
            }
            return packet;
        }
        catch (Exception e)
        {
            throw new BadSerializedDataException($"Bad packet\n{BitConverter.ToString(bytes.ToArray())}\n", e);
        }
    }

    protected abstract TPacket DeserializeCore(Stream stream);
}
