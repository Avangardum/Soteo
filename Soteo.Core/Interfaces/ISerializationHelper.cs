using System.Collections.Immutable;
using System.Numerics;
using Soteo.Core.Abilities;
using Soteo.Core.Dto;
using Soteo.Core.Statuses;

namespace Soteo.Core.Interfaces;

public interface ISerializationHelper
{
    delegate TElement Deserializer<out TElement>(Stream stream);
    delegate void Serializer<in TElement>(TElement value, Stream stream);

    void SerializeByte(byte value, Stream stream);
    byte DeserializeByte(Stream stream);
    void SerializeBool(bool value, Stream stream);
    bool DeserializeBool(Stream stream);
    void SerializeInt(int value, Stream stream);
    int DeserializeInt(Stream stream);
    void SerializeLong(long value, Stream stream);
    long DeserializeLong(Stream stream);
    void SerializeUShort(ushort value, Stream stream);
    ushort DeserializeUShort(Stream stream);
    void SerializeFloat(float value, Stream stream);
    float DeserializeFloat(Stream stream);
    void SerializeDouble(double value, Stream stream);
    double DeserializeDouble(Stream stream);
    void SerializeVector2(Vector2 value, Stream stream);
    Vector2 DeserializeVector2(Stream stream);
    void SerializeGuid(Guid value, Stream stream);
    Guid DeserializeGuid(Stream stream);
    void SerializeEnum<TEnum>(TEnum value, Stream stream) where TEnum : Enum;
    TEnum DeserializeEnum<TEnum>(Stream stream) where TEnum : Enum;
    TEnum DeserializeEnumWithoutValidation<TEnum>(Stream stream) where TEnum : Enum;

    void SerializeList<TElement>
    (
        IReadOnlyCollection<TElement> value,
        Serializer<TElement> serializeElement,
        Stream stream
    );

    TElement[] DeserializeList<TElement>
    (
        Deserializer<TElement> deserializeElement,
        Stream stream
    );

    void SerializeString(string value, Stream stream);
    string DeserializeString(Stream stream);

    void SerializeNullableStruct<T>(T? nullable, Serializer<T> serializer, Stream stream) where T : struct;

    T? DeserializeNullableStruct<T>(Deserializer<T> deserializer, Stream stream) where T : struct;

    void SerializeNullableClass<T>(T? nullable, Serializer<T> serializer, Stream stream) where T : class;

    T? DeserializeNullableClass<T>(Deserializer<T> deserializer, Stream stream) where T : class;

    void SerializeDictionary<TKey, TValue>
    (
        IReadOnlyDictionary<TKey, TValue> dictionary,
        Serializer<TKey> serializeKey,
        Serializer<TValue> serializeValue,
        Stream stream
    );

    ImmutableDictionary<TKey, TValue> DeserializeDictionary<TKey, TValue> 
    (
        Deserializer<TKey> deserializeKey,
        Deserializer<TValue> deserializeValue,
        Stream stream
    ) where TKey : notnull;

    /// <summary>
    /// Serialize a dictionary where keys are derived from values
    /// </summary>
    void SerializeIndexedDictionary<TKey, TValue>
    (
        IReadOnlyDictionary<TKey, TValue> dictionary,
        Serializer<TValue> serializeValue,
        Stream stream
    );

    /// <summary>
    /// Deserialize a dictionary where keys are derived from values
    /// </summary>
    ImmutableDictionary<TKey, TValue> DeserializeIndexedDictionary<TKey, TValue>
    (
        Deserializer<TValue> deserializeValue,
        Func<TValue, TKey> keySelector,
        Stream stream
    ) where TKey : notnull;

    void SerializeAbility(Ability value, Stream stream);
    Ability DeserializeAbility(Stream stream);
    void SerializeStatus(Status value, Stream stream);
    Status DeserializeStatus(Stream stream);
    void SerializePuppetStatusContext(PuppetStatusContext value, Stream stream);
    PuppetStatusContext DeserializePuppetStatusContext(Stream stream);
    void SerializeAbilityUseProgress(AbilityUseProgress value, Stream stream);
    AbilityUseProgress DeserializeAbilityUseProgress(Stream stream);
    void SerializeAbilitySlotState(AbilitySlotState value, Stream stream);
    AbilitySlotState DeserializeAbilitySlotState(Stream stream);
}
