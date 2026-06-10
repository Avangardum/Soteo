using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Numerics;
using System.Text;
using Soteo.Core.Exceptions;

namespace Soteo.Core;

public static class SerializationHelper
{
    public delegate TElement Deserializer<out TElement>(Stream stream);
    
    public delegate void Serializer<in TElement>(TElement value, Stream stream);
    
    public static void SerializeByte(byte value, Stream stream) => stream.WriteByte(value);

    public static byte DeserializeByte(Stream stream) => stream.ReadExactlyByte();

    public static void SerializeBool(bool value, Stream stream) =>
        SerializeByte(value ? (byte)1 : (byte)0, stream);
    
    public static bool DeserializeBool(Stream stream)
    {
        return DeserializeByte(stream) switch
        {
            0 => false,
            1 => true,
            _ => throw new BadPacketException("Invalid bool")
        };
    }

    public static void SerializeInt(int value, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        stream.Write(buffer);
    }

    public static int DeserializeInt(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt32BigEndian(buffer);
    }
    
    public static void SerializeLong(long value, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        stream.Write(buffer);
    }

    public static long DeserializeLong(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt64BigEndian(buffer);
    }

    public static void SerializeUShort(ushort value, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        stream.Write(buffer);
    }

    public static ushort DeserializeUShort(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }

    public static void SerializeFloat(float value, Stream stream)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) buffer.Reverse();
        stream.Write(buffer);
    }

    public static float DeserializeFloat(Stream stream)
    {
        byte[] buffer = new byte[sizeof(float)];
        stream.ReadExactly(buffer);
        if (BitConverter.IsLittleEndian) buffer.Reverse();
        return BitConverter.ToSingle(buffer, 0);
    }
    
    public static void SerializeDouble(double value, Stream stream)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) buffer.Reverse();
        stream.Write(buffer);
    }
    
    public static double DeserializeDouble(Stream stream)
    {
        byte[] buffer = new byte[sizeof(double)];
        stream.ReadExactly(buffer);
        if (BitConverter.IsLittleEndian) buffer.Reverse();
        return BitConverter.ToDouble(buffer, 0);
    }

    public static void SerializeVector2(Vector2 value, Stream stream)
    {
        SerializeFloat(value.X, stream);
        SerializeFloat(value.Y, stream);
    }

    public static Vector2 DeserializeVector2(Stream stream)
    {
        float x = DeserializeFloat(stream);
        float y = DeserializeFloat(stream);
        return new(x, y);
    }

    public static void SerializeGuid(Guid value, Stream stream)
    {
        stream.Write(value.ToByteArray());
    }

    public static Guid DeserializeGuid(Stream stream)
    {
        byte[] buffer = new byte[Const.BytesInGuid];
        stream.ReadExactly(buffer);
        return new Guid(buffer);
    }

    public static void SerializeEnum<TEnum>(TEnum value, Stream stream) where TEnum : Enum
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

    public static TEnum DeserializeEnum<TEnum>(Stream stream) where TEnum : Enum
    {
        TEnum value = DeserializeEnumWithoutValidation<TEnum>(stream);
        if (!Enum.IsDefined(typeof(TEnum), value) && !typeof(TEnum).HasAttribute<FlagsAttribute>())
            throw new BadPacketException($"Invalid {typeof(TEnum)} value {value}");
        return value;
    }

    public static TEnum DeserializeEnumWithoutValidation<TEnum>(Stream stream) where TEnum : Enum
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

    public static void SerializeList<TElement>
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

    public static TElement[] DeserializeList<TElement>
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

    public static void SerializeString(string value, Stream stream)
    {
        SerializeInt(Encoding.UTF8.GetByteCount(value), stream);
        stream.Write(Encoding.UTF8.GetBytes(value));
    }

    public static string DeserializeString(Stream stream)
    {
        int byteCount = DeserializeInt(stream);
        if (byteCount < 0 || byteCount > stream.Length - stream.Position)
            throw new BadPacketException("Invalid string length");
        byte[] buffer = new byte[byteCount];
        stream.ReadExactly(buffer);
        return Encoding.UTF8.GetString(buffer);
    }
    
    public static void SerializeNullableStruct<T>(T? nullable, Serializer<T> serializer, Stream stream)
        where T : struct
    {
        SerializeBool(nullable != null, stream);
        if (nullable != null)
            serializer(nullable.Value, stream);
    }
    
    public static T? DeserializeNullableStruct<T>(Deserializer<T> deserializer, Stream stream)
        where T : struct
    {
        bool hasValue = DeserializeBool(stream);
        return hasValue ? deserializer(stream) : null;
    }
    
    public static void SerializeNullableClass<T>(T? nullable, Serializer<T> serializer, Stream stream)
        where T : class
    {
        SerializeBool(nullable != null, stream);
        if (nullable != null)
            serializer(nullable, stream);
    }
    
    public static T? DeserializeNullableClass<T>(Deserializer<T> deserializer, Stream stream)
        where T : class
    {
        bool hasValue = DeserializeBool(stream);
        return hasValue ? deserializer(stream) : null;
    }

    public static void SerializeDictionary<TKey, TValue>
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

    public static ImmutableDictionary<TKey, TValue> DeserializeDictionary<TKey, TValue> 
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
    public static void SerializeIndexedDictionary<TKey, TValue>
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
    public static ImmutableDictionary<TKey, TValue> DeserializeIndexedDictionary<TKey, TValue>
    (
        Deserializer<TValue> deserializeValue,
        Func<TValue, TKey> keySelector,
        Stream stream
    ) where TKey: notnull
    {
        return DeserializeList(deserializeValue, stream).ToImmutableDictionary(keySelector, it => it);
    }
}
