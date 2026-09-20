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
        return typeLocator
            // TODO filtering by generic base type is used to filter out the routing serializer, make it explicit
            .ConcreteSubclassesOf<IPacketSerializer>(where: it => it.BaseType.Required.IsGenericType)
            .ToImmutableDictionary<Type, Type>(it => it.GetPacketType(typeof(PacketSerializer<>)));
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
