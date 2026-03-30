using System.Buffers.Binary;
using System.Numerics;
using System.Text;
using Soteo.Shared.Enums;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.PacketSerializers;

public abstract class PacketSerializer<TPacket> : IPacketSerializer
    where TPacket : Packet, new()
{
    protected delegate TElement Deserializer<out TElement>(ref Span<byte> span);
    
    protected delegate void Serializer<in TElement>(TElement value, ref Span<byte> span);
    
    protected virtual int PacketSize(TPacket packet) => sizeof(PacketType) + Const.BytesInGuid;
    
    byte[] IPacketSerializer.Serialize(Packet packet) => Serialize((TPacket)packet);
    
    public byte[] Serialize(TPacket packet)
    {
        var bytes = new byte[PacketSize(packet)];
        var span = new Span<byte>(bytes);
        SerializeInternal(packet, ref span);
        return bytes;
    }
    
    protected virtual void SerializeInternal(TPacket packet, ref Span<byte> span)
    {
        SerializeEnum(packet.Type, ref span);
        SerializeGuid(packet.CorrelationId, ref span);
    }
    
    Packet IPacketSerializer.Deserialize(Span<byte> bytes) => Deserialize(bytes);
    
    public TPacket Deserialize(Span<byte> bytes)
    {
        TPacket packet = DeserializeInternal(ref bytes);
        if (!bytes.IsEmpty) throw new BadPacketException("Packet contains extra bytes");
        return packet;
    }
    
    protected virtual TPacket DeserializeInternal(ref Span<byte> span)
    {
        var packet = new TPacket();
        var type = DeserializeEnum<PacketType>(ref span);
        if (type != packet.Type) throw new InvalidOperationException("Wrong serializer");
        packet.CorrelationId = DeserializeGuid(ref span);
        return packet;
    }
    
    protected Span<byte> SliceOff(int count, ref Span<byte> span)
    {
        if (count > span.Length) throw new BadPacketException("Packet is missing bytes");
        Span<byte> result = span[..count];
        span = span[count..];
        return result;
    }
    
    protected byte DeserializeByte(ref Span<byte> span) => SliceOff(1, ref span)[0];
    
    protected void SerializeByte(byte value, ref Span<byte> span)
    {
        Span<byte> slice = SliceOff(1, ref span);
        slice[0] = value;
    }
    
    protected int DeserializeInt(ref Span<byte> span)
    {
        var slice = SliceOff(sizeof(int), ref span);
        return BinaryPrimitives.ReadInt32BigEndian(slice);
    }
    
    protected void SerializeInt(int value, ref Span<byte> span)
    {
        var slice = SliceOff(sizeof(int), ref span);
        BinaryPrimitives.WriteInt32BigEndian(slice, value);
    }
    
    protected ushort DeserializeUShort(ref Span<byte> span)
    {
        var slice = SliceOff(sizeof(ushort), ref span);
        return BinaryPrimitives.ReadUInt16BigEndian(slice);
    }
    
    protected void SerializeUShort(ushort value, ref Span<byte> span)
    {
        var slice = SliceOff(sizeof(ushort), ref span);
        BinaryPrimitives.WriteUInt16BigEndian(slice, value);
    }
    
    protected char DeserializeChar(ref Span<byte> span) => (char)DeserializeUShort(ref span);
    
    protected void SerializeChar(char value, ref Span<byte> span) => SerializeUShort(value, ref span);

    protected Guid DeserializeGuid(ref Span<byte> span)
    {
        Span<byte> slice = SliceOff(Const.BytesInGuid, ref span);
        return new Guid(slice.ToArray());
    }

    protected void SerializeGuid(Guid value, ref Span<byte> span)
    {
        var slice = SliceOff(Const.BytesInGuid, ref span);
        value.ToByteArray().CopyTo(slice);
    }
    
    protected TEnum DeserializeEnum<TEnum>(ref Span<byte> span) where TEnum : Enum
    {
        TEnum value = DeserializeEnumWithoutValidation<TEnum>(ref span);
        if (!Enum.IsDefined(typeof(TEnum), value) && !typeof(TEnum).HasAttribute<FlagsAttribute>())
            throw new BadPacketException("Invalid enum value");
        return value;
    }
    
    protected TEnum DeserializeEnumWithoutValidation<TEnum>(ref Span<byte> span) where TEnum : Enum
    {
        Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        if (underlyingType == typeof(byte)) return (TEnum)(object)DeserializeByte(ref span);
        throw new NotSupportedException();
    }
    
    protected void SerializeEnum<TEnum>(TEnum value, ref Span<byte> span) where TEnum : Enum
    {
        Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        if (underlyingType == typeof(byte)) SerializeByte((byte)(object)value, ref span);
        else throw new NotSupportedException();
    }
    
    protected TElement[] DeserializeArray<TElement>(Deserializer<TElement> deserializeValue, ref Span<byte> span)
        where TElement : unmanaged
    {
        int length = DeserializeInt(ref span);
        int maxLength = span.Length / SizeOf<TElement>();
        if (length < 0 || length > maxLength) throw new BadPacketException("Invalid array length");
        var result = new TElement[length];
        for (int i = 0; i < length; i++) result[i] = deserializeValue(ref span);
        return result;
    }
    
    protected void SerializeArray<TElement>
    (
        Span<TElement> value,
        Serializer<TElement> serializeElement,
        ref Span<byte> span
    ) where TElement : unmanaged
    {
        SerializeInt(value.Length, ref span);
        for (var i = 0; i < value.Length; i++) serializeElement(value[i], ref span);
    }
    
    protected string DeserializeString(ref Span<byte> span)
    {
        int byteCount = DeserializeInt(ref span);
        if (byteCount < 0 || byteCount > span.Length) throw new BadPacketException("Invalid string length");
        Span<byte> slice = SliceOff(byteCount, ref span);
        return Encoding.UTF8.GetString(slice.ToArray());
    }
    
    protected void SerializeString(string value, ref Span<byte> span)
    {
        int byteCount = Encoding.UTF8.GetByteCount(value);
        SerializeInt(byteCount, ref span);
        Span<byte> slice = SliceOff(byteCount, ref span);
        Encoding.UTF8.GetBytes(value).CopyTo(slice);
    }

    protected int SizeOf<TValue>(TValue value = default) where TValue : unmanaged
    {
        return value switch
        {
            byte or sbyte or bool => 1,
            short or ushort or char => 2,
            int or uint or float => 4,
            long or ulong or double or Vector2 => 8,
            Guid or decimal => 16,
            _ => throw new NotSupportedException()
        };
    }
    
    protected int SizeOf<TElement>(TElement[] array) where TElement : unmanaged =>
        sizeof(int) + array.Length * SizeOf<TElement>();

    protected int SizeOf(string s) => sizeof(int) + Encoding.UTF8.GetByteCount(s);
}