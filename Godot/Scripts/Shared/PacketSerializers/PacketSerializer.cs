using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Text;
using Soteo.Shared.Enums;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public abstract class PacketSerializer<TPacket> : IPacketSerializer
    where TPacket : Packet, new()
{
    protected delegate TElement Deserializer<out TElement>(Stream stream);
    
    protected delegate void Serializer<in TElement>(TElement value, Stream stream);
    
    byte[] IPacketSerializer.Serialize(Packet packet) => Serialize((TPacket)packet);
    
    public byte[] Serialize(TPacket packet)
    {
        var stream = new MemoryStream();
        SerializeInternal(packet, stream);
        return stream.ToArray();
    }
    
    protected virtual void SerializeInternal(TPacket packet, Stream stream)
    {
        SerializeEnum(packet.Type, stream);
    }
    
    Packet IPacketSerializer.Deserialize(Span<byte> bytes) => Deserialize(bytes);
    
    public TPacket Deserialize(Span<byte> bytes)
    {
        try
        {
            var stream = new MemoryStream(bytes.ToArray());
            TPacket packet = DeserializeInternal(stream);
            if (stream.Position != bytes.Length) throw new BadPacketException(
                $"Packet deserialized as {packet}, but contains {bytes.Length - stream.Position} extra bytes");
            return packet;
        }
        catch (BadPacketException e)
        {
            throw new AggregateException($"Bad packet\n{BitConverter.ToString(bytes.ToArray())}\n", e);
        }
    }
    
    protected virtual TPacket DeserializeInternal(Stream stream)
    {
        var packet = new TPacket();
        var type = DeserializeEnum<PacketType>(stream);
        if (type != packet.Type)
            throw new InvalidOperationException("Wrong serializer");
        return packet;
    }
    
    protected void SerializeByte(byte value, Stream stream) => stream.WriteByte(value);

    protected byte DeserializeByte(Stream stream) => stream.ReadExactlyByte();

    protected void SerializeBool(bool value, Stream stream) =>
        SerializeByte(value ? (byte)1 : (byte)0, stream);
    
    protected bool DeserializeBool(Stream stream)
    {
        return DeserializeByte(stream) switch
        {
            0 => false,
            1 => true,
            _ => throw new BadPacketException("Invalid bool")
        };
    }

    protected void SerializeInt(int value, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        stream.Write(buffer);
    }

    protected int DeserializeInt(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt32BigEndian(buffer);
    }
    
    protected void SerializeLong(long value, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        stream.Write(buffer);
    }

    protected long DeserializeLong(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt64BigEndian(buffer);
    }

    protected void SerializeUShort(ushort value, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        stream.Write(buffer);
    }

    protected ushort DeserializeUShort(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }

    protected void SerializeFloat(float value, Stream stream)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) buffer.Reverse();
        stream.Write(buffer);
    }

    protected float DeserializeFloat(Stream stream)
    {
        byte[] buffer = new byte[sizeof(float)];
        stream.ReadExactly(buffer);
        if (BitConverter.IsLittleEndian) buffer.Reverse();
        return BitConverter.ToSingle(buffer, 0);
    }
    
    protected void SerializeDouble(double value, Stream stream)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) buffer.Reverse();
        stream.Write(buffer);
    }
    
    protected double DeserializeDouble(Stream stream)
    {
        byte[] buffer = new byte[sizeof(double)];
        stream.ReadExactly(buffer);
        if (BitConverter.IsLittleEndian) buffer.Reverse();
        return BitConverter.ToDouble(buffer, 0);
    }

    protected void SerializeVector2(Vector2 value, Stream stream)
    {
        SerializeFloat(value.x, stream);
        SerializeFloat(value.y, stream);
    }

    protected Vector2 DeserializeVector2(Stream stream)
    {
        float x = DeserializeFloat(stream);
        float y = DeserializeFloat(stream);
        return new(x, y);
    }

    protected void SerializeGuid(Guid value, Stream stream)
    {
        stream.Write(value.ToByteArray());
    }

    protected Guid DeserializeGuid(Stream stream)
    {
        byte[] buffer = new byte[Const.BytesInGuid];
        stream.ReadExactly(buffer);
        return new Guid(buffer);
    }

    protected void SerializeEnum<TEnum>(TEnum value, Stream stream) where TEnum : Enum
    {
        Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        if (underlyingType == typeof(byte))
            SerializeByte((byte)(object)value, stream);
        else if (underlyingType == typeof(ushort))
            SerializeUShort((ushort)(object)value, stream);
        else if (underlyingType == typeof(int))
            SerializeInt((int)(object)value, stream);
        else
            throw new NotSupportedException();
    }

    protected TEnum DeserializeEnum<TEnum>(Stream stream) where TEnum : Enum
    {
        TEnum value = DeserializeEnumWithoutValidation<TEnum>(stream);
        if (!Enum.IsDefined(typeof(TEnum), value) && !typeof(TEnum).HasAttribute<FlagsAttribute>())
            throw new BadPacketException($"Invalid {typeof(TEnum)} value {value}");
        return value;
    }

    protected TEnum DeserializeEnumWithoutValidation<TEnum>(Stream stream) where TEnum : Enum
    {
        Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
        if (underlyingType == typeof(byte))
            return (TEnum)(object)DeserializeByte(stream);
        if (underlyingType == typeof(ushort))
            return (TEnum)(object)DeserializeUShort(stream);
        if (underlyingType == typeof(int))
            return (TEnum)(object)DeserializeInt(stream);
        throw new NotSupportedException();
    }

    protected void SerializeList<TElement>
    (
        IReadOnlyCollection<TElement> value,
        Serializer<TElement> serializeElement,
        Stream stream
    )
    {
        SerializeInt(value.Count, stream);
        
        if (value is byte[] bytes)
        {
            stream.Write(bytes);
        }
        else
        {
            foreach (var element in value)
                serializeElement(element, stream);
        }
    }

    protected TElement[] DeserializeList<TElement>
    (
        Deserializer<TElement> deserializeElement,
        Stream stream
    )
    {
        int length = DeserializeInt(stream);
        if (length < 0 || length > stream.Length - stream.Position)
            throw new BadPacketException("Invalid list length");
        var result = new TElement[length];
        if (typeof(TElement) == typeof(byte))
        {
            stream.ReadExactly((byte[])(object)result);
        }
        else
        {
            for (int i = 0; i < length; i++)
                result[i] = deserializeElement(stream);
        }
        return result;
    }

    protected void SerializeString(string value, Stream stream)
    {
        SerializeInt(Encoding.UTF8.GetByteCount(value), stream);
        stream.Write(Encoding.UTF8.GetBytes(value));
    }

    protected string DeserializeString(Stream stream)
    {
        int byteCount = DeserializeInt(stream);
        if (byteCount < 0 || byteCount > stream.Length - stream.Position)
            throw new BadPacketException("Invalid string length");
        byte[] buffer = new byte[byteCount];
        stream.ReadExactly(buffer);
        return Encoding.UTF8.GetString(buffer);
    }
    
    protected void SerializeNullableStruct<T>(T? nullable, Serializer<T> serializer, Stream stream)
        where T : struct
    {
        SerializeBool(nullable != null, stream);
        if (nullable != null)
            serializer(nullable.Value, stream);
    }
    
    protected T? DeserializeNullableStruct<T>(Deserializer<T> deserializer, Stream stream)
        where T : struct
    {
        bool hasValue = DeserializeBool(stream);
        return hasValue ? deserializer(stream) : null;
    }
    
    protected void SerializeNullableClass<T>(T? nullable, Serializer<T> serializer, Stream stream)
        where T : class
    {
        SerializeBool(nullable != null, stream);
        if (nullable != null)
            serializer(nullable, stream);
    }
    
    protected T? DeserializeNullableClass<T>(Deserializer<T> deserializer, Stream stream)
        where T : class
    {
        bool hasValue = DeserializeBool(stream);
        return hasValue ? deserializer(stream) : null;
    }

    protected void SerializeDictionary<TKey, TValue>
    (
        IReadOnlyDictionary<TKey, TValue> dictionary,
        Serializer<TKey> serializeKey,
        Serializer<TValue> serializeValue,
        Stream stream
    )
    {
        SerializeList(dictionary, (pair, _) =>
        {
            serializeKey(pair.Key, stream);
            serializeValue(pair.Value, stream);
        }, stream);
    }

    protected ImmutableDictionary<TKey, TValue> DeserializeDictionary<TKey, TValue> 
    (
        Deserializer<TKey> deserializeKey,
        Deserializer<TValue> deserializeValue,
        Stream stream
    ) where TKey : notnull
    {
        return DeserializeList
        (
            _ => new KeyValuePair<TKey, TValue>(deserializeKey(stream), deserializeValue(stream)),
            stream
        ).ToImmutableDictionary();
    }
    
    /// <summary>
    /// Serialize a dictionary where keys are derived from values
    /// </summary>
    protected void SerializeIndexedDictionary<TKey, TValue>
    (
        IReadOnlyDictionary<TKey, TValue> dictionary,
        Serializer<TValue> serializeValue,
        Stream stream
    )
    {
        SerializeList(dictionary.Values.ToList(), serializeValue, stream);
    }
    
    /// <summary>
    /// Deserialize a dictionary where keys are derived from values
    /// </summary>
    protected ImmutableDictionary<TKey, TValue> DeserializeIndexedDictionary<TKey, TValue>
    (
        Deserializer<TValue> deserializeValue,
        Func<TValue, TKey> keySelector,
        Stream stream
    ) where TKey: notnull
    {
        return DeserializeList(deserializeValue, stream).ToImmutableDictionary(keySelector, it => it);
    }
    
    // todo refactor parameter order
}