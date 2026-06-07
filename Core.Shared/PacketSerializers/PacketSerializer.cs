using System.Collections.Immutable;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Exceptions;
using Soteo.Core.Shared.Extensions;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Shared.PacketSerializers;

public static class PacketSerializer
{
    private static ImmutableDictionary<PacketTypeCode, IPacketSerializer>? _instancesByPacketType;
    
    public static IPacketSerializer For(PacketTypeCode packetTypeCode, ITypeLocator typeLocator)
    {
        _instancesByPacketType ??= typeLocator
            .InstanceSubclassesOf<IPacketSerializer>(where: it => it.BaseType.Required.IsGenericType)
            .ToImmutableDictionary(it => it.GetType().GetPacketType(typeof(PacketSerializer<>)));
        return _instancesByPacketType[packetTypeCode];
    }
}

public abstract class PacketSerializer<TPacket> : IPacketSerializer where TPacket : Packet
{
    public static readonly PacketTypeCode PacketTypeCode =
        typeof(TPacket).GetRequiredAttribute<PacketTypeCodeAttribute>().TypeCode;
    
    byte[] IPacketSerializer.Serialize(Packet packet) => Serialize((TPacket)packet);
    
    public byte[] Serialize(TPacket packet)
    {
        var stream = new MemoryStream();
        SerializeEnum(packet.TypeCode, stream);
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
            var typeCode = DeserializeEnum<PacketTypeCode>(stream);
            if (typeCode != PacketTypeCode)
                throw new InvalidOperationException("Wrong serializer");
            TPacket packet = DeserializeInternal(stream);
            if (stream.Position != bytes.Length)
            {
                throw new BadPacketException
                (
                    $"Packet deserialized as {packet}, but contains {bytes.Length - stream.Position} extra bytes"
                );
            }
            return packet;
        }
        catch (BadPacketException e)
        {
            throw new AggregateException($"Bad packet\n{BitConverter.ToString(bytes.ToArray())}\n", e);
        }
    }
    
    protected abstract TPacket DeserializeInternal(Stream stream);
}
