using System.Collections.Immutable;
using Soteo.Core.Attributes;
using Soteo.Core.Enums;
using Soteo.Core.Exceptions;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketSerializers;

public static class PacketSerializer
{
    private static ImmutableDictionary<PacketTypeCode, Type>? _typesByPacketType;
    
    public static Type? TypeFor(PacketTypeCode packetTypeCode, ITypeLocator typeLocator)
    {
        _typesByPacketType ??= InitTypesByPacketType(typeLocator);
        return _typesByPacketType.GetOrDefault(packetTypeCode);
    }
    
    public static IReadOnlyList<Type> AllTypes(ITypeLocator typeLocator)
    {
        _typesByPacketType ??= InitTypesByPacketType(typeLocator);
        return _typesByPacketType.Values.ToImmutableList();
    }
    
    private static ImmutableDictionary<PacketTypeCode, Type> InitTypesByPacketType(ITypeLocator typeLocator)
    {
        return typeLocator
            .ConcreteSubclassesOf<IPacketSerializer>(where: it => it.BaseType.Required.IsGenericType)
            .ToImmutableDictionary<Type, PacketTypeCode>(it => it.GetPacketType(typeof(PacketSerializer<>)));
    }
}

public abstract class PacketSerializer<TPacket>(ISerializationHelper s) : IPacketSerializer where TPacket : Packet
{
    public static readonly PacketTypeCode PacketTypeCode =
        typeof(TPacket).GetRequiredAttribute<PacketTypeCodeAttribute>().TypeCode;
    
    byte[] IPacketSerializer.Serialize(Packet packet) => Serialize((TPacket)packet);
    
    public byte[] Serialize(TPacket packet)
    {
        var stream = new MemoryStream();
        s.SerializeEnum(packet.TypeCode, stream);
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
            var typeCode = s.DeserializeEnum<PacketTypeCode>(stream);
            if (typeCode != PacketTypeCode)
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
            throw new BadSerializedDataException($"Bad packet\n{BitConverter.ToString(bytes.ToArray())}\n{e.Message}");
        }
    }
    
    protected abstract TPacket DeserializeInternal(Stream stream);
}
