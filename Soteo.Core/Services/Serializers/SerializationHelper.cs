using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Numerics;
using System.Text;
using Soteo.Core.Abilities;
using Soteo.Core.Delegates;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Exceptions;
using Soteo.Core.Interfaces;
using Soteo.Core.PacketSerializers;
using Soteo.Core.Statuses;

namespace Soteo.Core;

public class SerializationHelper(ITypeLocator typeLocator) : ISerializationHelper
{
    private readonly IReadOnlyList<Type> _abilityTypes = typeLocator.ConcreteSubclassesOf<Ability>();
    private readonly IReadOnlyList<Type> _statusTypes = typeLocator.ConcreteSubclassesOf<Status>();

    public void SerializeByte(byte value, Stream stream) => stream.WriteByte(value);

    public byte DeserializeByte(Stream stream) => stream.ReadExactlyByte();

    public void SerializeBool(bool value, Stream stream) =>
        SerializeByte(value ? (byte)1 : (byte)0, stream);
    
    public bool DeserializeBool(Stream stream)
    {
        return DeserializeByte(stream) switch
        {
            0 => false,
            1 => true,
            _ => throw new BadSerializedDataException("Invalid bool")
        };
    }

    public void SerializeInt(int value, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        stream.Write(buffer);
    }

    public int DeserializeInt(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt32BigEndian(buffer);
    }
    
    public void SerializeLong(long value, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        stream.Write(buffer);
    }

    public long DeserializeLong(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt64BigEndian(buffer);
    }

    public void SerializeUShort(ushort value, Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        stream.Write(buffer);
    }

    public ushort DeserializeUShort(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }

    public void SerializeFloat(float value, Stream stream)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) buffer.Reverse();
        stream.Write(buffer);
    }

    public float DeserializeFloat(Stream stream)
    {
        byte[] buffer = new byte[sizeof(float)];
        stream.ReadExactly(buffer);
        if (BitConverter.IsLittleEndian) buffer.Reverse();
        return BitConverter.ToSingle(buffer, 0);
    }
    
    public void SerializeDouble(double value, Stream stream)
    {
        byte[] buffer = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) buffer.Reverse();
        stream.Write(buffer);
    }
    
    public double DeserializeDouble(Stream stream)
    {
        byte[] buffer = new byte[sizeof(double)];
        stream.ReadExactly(buffer);
        if (BitConverter.IsLittleEndian) buffer.Reverse();
        return BitConverter.ToDouble(buffer, 0);
    }

    public void SerializeVector2(Vector2 value, Stream stream)
    {
        SerializeFloat(value.X, stream);
        SerializeFloat(value.Y, stream);
    }

    public Vector2 DeserializeVector2(Stream stream)
    {
        float x = DeserializeFloat(stream);
        float y = DeserializeFloat(stream);
        return new(x, y);
    }

    public void SerializeGuid(Guid value, Stream stream)
    {
        stream.Write(value.ToByteArray());
    }

    public Guid DeserializeGuid(Stream stream)
    {
        byte[] buffer = new byte[Const.BytesInGuid];
        stream.ReadExactly(buffer);
        return new Guid(buffer);
    }

    public void SerializeEnum<TEnum>(TEnum value, Stream stream) where TEnum : Enum
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

    public TEnum DeserializeEnum<TEnum>(Stream stream) where TEnum : Enum
    {
        TEnum value = DeserializeEnumWithoutValidation<TEnum>(stream);
        if (!Enum.IsDefined(typeof(TEnum), value) && !typeof(TEnum).HasAttribute<FlagsAttribute>())
            throw new BadSerializedDataException($"Invalid {typeof(TEnum)} value {value}");
        return value;
    }

    public TEnum DeserializeEnumWithoutValidation<TEnum>(Stream stream) where TEnum : Enum
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

    public void SerializeList<TElement>
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

    public TElement[] DeserializeList<TElement>
    (
        Deserializer<TElement> deserializeElement,
        Stream stream
    )
    {
        int length = DeserializeInt(stream);
        if (length < 0 || length > stream.Length - stream.Position)
            throw new BadSerializedDataException("Invalid list length");
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

    public void SerializeString(string value, Stream stream)
    {
        SerializeInt(Encoding.UTF8.GetByteCount(value), stream);
        stream.Write(Encoding.UTF8.GetBytes(value));
    }

    public string DeserializeString(Stream stream)
    {
        int byteCount = DeserializeInt(stream);
        if (byteCount < 0 || byteCount > stream.Length - stream.Position)
            throw new BadSerializedDataException("Invalid string length");
        byte[] buffer = new byte[byteCount];
        stream.ReadExactly(buffer);
        return Encoding.UTF8.GetString(buffer);
    }
    
    public void SerializeNullableStruct<T>(T? nullable, Serializer<T> serializer, Stream stream)
        where T : struct
    {
        SerializeBool(nullable != null, stream);
        if (nullable != null)
            serializer(nullable.Value, stream);
    }
    
    public T? DeserializeNullableStruct<T>(Deserializer<T> deserializer, Stream stream)
        where T : struct
    {
        bool hasValue = DeserializeBool(stream);
        return hasValue ? deserializer(stream) : null;
    }
    
    public void SerializeNullableClass<T>(T? nullable, Serializer<T> serializer, Stream stream)
        where T : class
    {
        SerializeBool(nullable != null, stream);
        if (nullable != null)
            serializer(nullable, stream);
    }
    
    public T? DeserializeNullableClass<T>(Deserializer<T> deserializer, Stream stream)
        where T : class
    {
        bool hasValue = DeserializeBool(stream);
        return hasValue ? deserializer(stream) : null;
    }

    public void SerializeDictionary<TKey, TValue>
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

    public ImmutableDictionary<TKey, TValue> DeserializeDictionary<TKey, TValue> 
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
    public void SerializeIndexedDictionary<TKey, TValue>
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
    public ImmutableDictionary<TKey, TValue> DeserializeIndexedDictionary<TKey, TValue>
    (
        Deserializer<TValue> deserializeValue,
        Func<TValue, TKey> keySelector,
        Stream stream
    ) where TKey : notnull
    {
        return DeserializeList(deserializeValue, stream).ToImmutableDictionary(keySelector, it => it);
    }
    
    public void SerializeAbility(Ability value, Stream stream) =>
        SerializeInt(_abilityTypes.IndexOf(value.GetType()), stream);
    
    public Ability DeserializeAbility(Stream stream) =>
        Ability.Instance(_abilityTypes[DeserializeInt(stream)]);
    
    public void SerializeStatus(Status value, Stream stream) =>
        SerializeInt(_statusTypes.IndexOf(value.GetType()), stream);
    
    public Status DeserializeStatus(Stream stream) =>
        Status.Instance(_statusTypes[DeserializeInt(stream)]);
    
    public void SerializePuppetStatusContext(PuppetStatusContext value, Stream stream)
    {
        SerializeGuid(value.Id, stream);
        SerializeStatus(value.Status, stream);
        SerializeNullableClass(value.Ability, SerializeAbility, stream);
        SerializeDouble(value.DisplayElapsedTime, stream);
        SerializeDouble(value.RemainingTime, stream);
        SerializeLong(value.Ordinal, stream);
    }
    
    public PuppetStatusContext DeserializePuppetStatusContext(Stream stream)
    {
        return new PuppetStatusContext
        {
            Id = DeserializeGuid(stream),
            Status = DeserializeStatus(stream),
            Ability = DeserializeNullableClass(DeserializeAbility, stream),
            DisplayElapsedTime = DeserializeDouble(stream),
            RemainingTime = DeserializeDouble(stream),
            Ordinal = DeserializeLong(stream),
        };
    }
    
    public void SerializeAbilityUseProgress(AbilityUseProgress value, Stream stream)
    {
        SerializeEnum(value.Slot, stream);
        SerializeDouble(value.ElapsedTime, stream);
        SerializeDouble(value.RemainingTime, stream);
    }
    
    public AbilityUseProgress DeserializeAbilityUseProgress(Stream stream)
    {
        return new AbilityUseProgress
        {
            Slot = DeserializeEnum<AbilitySlot>(stream),
            ElapsedTime = DeserializeDouble(stream),
            RemainingTime = DeserializeDouble(stream),
        };
    }
    
    public void SerializeAbilitySlotState(AbilitySlotState value, Stream stream)
    {
        SerializeAbility(value.Ability, stream);
        SerializeInt(value.Level, stream);
        SerializeDouble(value.Cooldown, stream);
        SerializeDouble(value.MaxCooldown, stream);
    }
    
    public AbilitySlotState DeserializeAbilitySlotState(Stream stream)
    {
        return new AbilitySlotState
        {
            Ability = DeserializeAbility(stream),
            Level = DeserializeInt(stream),
            Cooldown = DeserializeDouble(stream),
            MaxCooldown = DeserializeDouble(stream),
        };
    }

    public void SerializeEntitySnapshot(EntitySnapshot entity, Stream stream)
    {
        switch (entity)
        {
            case UnitSnapshot unit:
                SerializeUnitSnapshot(unit, stream);
                break;
            case ProjectileSnapshot projectile:
                SerializeProjectileSnapshot(projectile, stream);
                break;
            case UnitPuppetSnapshot unitPuppet:
                SerializeUnitPuppetSnapshot(unitPuppet, stream);
                break;
            case ProjectilePuppetSnapshot projectilePuppet:
                SerializeProjectilePuppetSnapshot(projectilePuppet, stream);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public EntitySnapshot DeserializeEntitySnapshot(Stream stream)
    {
        return DeserializeEnum<EntityKind>(stream) switch
        {
            EntityKind.Unit => DeserializeUnitSnapshot(stream),
            EntityKind.Projectile => DeserializeProjectileSnapshot(stream),
            EntityKind.UnitPuppet => DeserializeUnitPuppetSnapshot(stream),
            EntityKind.ProjectilePuppet => DeserializeProjectilePuppetSnapshot(stream),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
    
    private void SerializeBaseEntitySnapshot(EntitySnapshot entity, Stream stream)
    {
        SerializeGuid(entity.Id, stream);
        SerializeBool(entity.IsRemoved, stream);
        SerializeVector2(entity.Position, stream);
        SerializeDouble(entity.Azimuth, stream);
    }

    public void SerializeUnitSnapshot(UnitSnapshot unit, Stream stream)
    {
        SerializeEnum(EntityKind.Unit, stream);
        SerializeBaseEntitySnapshot(unit, stream);
        SerializeBool(unit.IsDead, stream);
        SerializeBool(unit.IsMoving, stream);
        SerializeDictionary(unit.Stats, SerializeEnum, SerializeDouble, stream);
        SerializeDictionary(unit.AbilitySlotStates, SerializeEnum, SerializeAbilitySlotState, stream);
        SerializeNullableClass(unit.AbilityUseProgress, SerializeAbilityUseProgress, stream);
        SerializeIndexedDictionary(unit.Statuses, SerializeDeflatedStatusContext, stream);
    }

    public UnitSnapshot DeserializeUnitSnapshot(Stream stream)
    {
        return new UnitSnapshot
        {
            Id = DeserializeGuid(stream),
            IsRemoved = DeserializeBool(stream),
            Position = DeserializeVector2(stream),
            Azimuth = DeserializeDouble(stream),
            IsDead = DeserializeBool(stream),
            IsMoving = DeserializeBool(stream),
            Stats = DeserializeDictionary(DeserializeEnum<Stat>, DeserializeDouble, stream),
            AbilitySlotStates =
                DeserializeDictionary(DeserializeEnum<AbilitySlot>, DeserializeAbilitySlotState, stream),
            AbilityUseProgress = DeserializeNullableClass(DeserializeAbilityUseProgress, stream),
            Statuses = DeserializeIndexedDictionary(DeserializeDeflatedStatusContext, it => it.Id, stream)
        };
    }

    public void SerializeUnitPuppetSnapshot(UnitPuppetSnapshot unitPuppet, Stream stream)
    {
        SerializeEnum(EntityKind.UnitPuppet, stream);
        SerializeBaseEntitySnapshot(unitPuppet, stream);
        SerializeBool(unitPuppet.IsDead, stream);
        SerializeBool(unitPuppet.IsMoving, stream);
        SerializeDictionary(unitPuppet.Stats, SerializeEnum, SerializeDouble, stream);
        SerializeDictionary(unitPuppet.AbilitySlotStates, SerializeEnum, SerializeAbilitySlotState, stream);
        SerializeNullableClass(unitPuppet.AbilityUseProgress, SerializeAbilityUseProgress, stream);
        SerializeIndexedDictionary(unitPuppet.Statuses, SerializePuppetStatusContext, stream);
    }

    public UnitPuppetSnapshot DeserializeUnitPuppetSnapshot(Stream stream)
    {
        return new UnitPuppetSnapshot
        {
            Id = DeserializeGuid(stream),
            IsRemoved = DeserializeBool(stream),
            Position = DeserializeVector2(stream),
            Azimuth = DeserializeDouble(stream),
            IsDead = DeserializeBool(stream),
            IsMoving = DeserializeBool(stream),
            Stats = DeserializeDictionary(DeserializeEnum<Stat>, DeserializeDouble, stream),
            AbilitySlotStates =
                DeserializeDictionary(DeserializeEnum<AbilitySlot>, DeserializeAbilitySlotState, stream),
            AbilityUseProgress = DeserializeNullableClass(DeserializeAbilityUseProgress, stream),
            Statuses = DeserializeIndexedDictionary(DeserializePuppetStatusContext, it => it.Id, stream)
        };
    }

    public void SerializeProjectileSnapshot(ProjectileSnapshot projectile, Stream stream)
    {
        SerializeEnum(EntityKind.Projectile, stream);
        SerializeBaseEntitySnapshot(projectile, stream);
        SerializeDouble(projectile.Speed, stream);
        SerializeDeflatedAbilityContext(projectile.AbilityContext, stream);
        SerializeProjectileTarget(projectile.Target, stream);
    }

    public ProjectileSnapshot DeserializeProjectileSnapshot(Stream stream)
    {
        return new ProjectileSnapshot
        {
            Id = DeserializeGuid(stream),
            IsRemoved = DeserializeBool(stream),
            Position = DeserializeVector2(stream),
            Azimuth = DeserializeDouble(stream),
            Speed = DeserializeDouble(stream),
            AbilityContext = DeserializeDeflatedAbilityContext(stream),
            Target = DeserializeProjectileTarget(stream),
        };
    }

    public void SerializeProjectileTarget(DeflatedProjectileTarget value, Stream stream)
    {
        SerializeBool(value.IsUnit, stream);
        if (value.IsUnit)
            SerializeGuid(value.UnitId.Value, stream);
        else
            SerializeVector2(value.Position.Value, stream);
    }

    public DeflatedProjectileTarget DeserializeProjectileTarget(Stream stream)
    {
        bool isUnit = DeserializeBool(stream);
        if (isUnit)
            return DeserializeGuid(stream);
        else
            return DeserializeVector2(stream);
    }

    public void SerializeProjectilePuppetSnapshot(ProjectilePuppetSnapshot projectilePuppet, Stream stream)
    {
        SerializeEnum(EntityKind.ProjectilePuppet, stream);
        SerializeBaseEntitySnapshot(projectilePuppet, stream);
    }

    public ProjectilePuppetSnapshot DeserializeProjectilePuppetSnapshot(Stream stream)
    {
        return new ProjectilePuppetSnapshot
        {
            Id = DeserializeGuid(stream),
            IsRemoved = DeserializeBool(stream),
            Position = DeserializeVector2(stream),
            Azimuth = DeserializeDouble(stream)
        };
    }

    public void SerializeDeflatedAbilityContext(DeflatedAbilityContext context, Stream stream)
    {
        SerializeAbility(context.Ability, stream);
        SerializeInt(context.Level, stream);
        SerializeGuid(context.UserId, stream);
        SerializeDictionary(context.UserStats, SerializeEnum, SerializeDouble, stream);
        SerializeNullableStruct(context.TargetPosition, SerializeVector2, stream);
        SerializeNullableStruct(context.TargetUnitId, SerializeGuid, stream);
        SerializeNullableStruct(context.TargetDirection, SerializeVector2, stream);
        SerializeNullableStruct(context.TargetShardId, SerializeGuid, stream);
    }

    public DeflatedAbilityContext DeserializeDeflatedAbilityContext(Stream stream)
    {
        return new DeflatedAbilityContext
        {
            Ability = DeserializeAbility(stream),
            Level = DeserializeInt(stream),
            UserId = DeserializeGuid(stream),
            UserStats = DeserializeDictionary(DeserializeEnum<Stat>, DeserializeDouble, stream),
            TargetPosition = DeserializeNullableStruct(DeserializeVector2, stream),
            TargetUnitId = DeserializeNullableStruct(DeserializeGuid, stream),
            TargetDirection = DeserializeNullableStruct(DeserializeVector2, stream),
            TargetShardId = DeserializeNullableStruct(DeserializeGuid, stream)
        };
    }

    public void SerializeDeflatedStatusContext(DeflatedStatusContext value, Stream stream)
    {
        SerializeGuid(value.Id, stream);
        SerializeStatus(value.Status, stream);
        SerializeNullableClass(value.AbilityContext, SerializeDeflatedAbilityContext, stream);
        SerializeGuid(value.UnitId, stream);
        SerializeNullableStruct(value.SourceId, SerializeGuid, stream);
        SerializeNullableClass(value.Tick, SerializeStatusTickContext, stream);
        SerializeDouble(value.ElapsedTime, stream);
        SerializeDouble(value.DisplayElapsedTime, stream);
        SerializeDouble(value.RemainingTime, stream);
        SerializeLong(value.Ordinal, stream);
    }

    public DeflatedStatusContext DeserializeDeflatedStatusContext(Stream stream)
    {
        return new DeflatedStatusContext
        {
            Id = DeserializeGuid(stream),
            Status = DeserializeStatus(stream),
            AbilityContext = DeserializeNullableClass(DeserializeDeflatedAbilityContext, stream),
            UnitId = DeserializeGuid(stream),
            SourceId = DeserializeNullableStruct(DeserializeGuid, stream),
            Tick = DeserializeNullableClass(DeserializeStatusTickContext, stream),
            ElapsedTime = DeserializeDouble(stream),
            DisplayElapsedTime = DeserializeDouble(stream),
            RemainingTime = DeserializeDouble(stream),
            Ordinal = DeserializeLong(stream),
        };
    }

    public void SerializeStatusTickContext(StatusTickContext value, Stream stream)
    {
        SerializeDouble(value.Interval, stream);
        SerializeDouble(value.Countdown, stream);
    }

    public StatusTickContext DeserializeStatusTickContext(Stream stream)
    {
        double interval = DeserializeDouble(stream);
        double countdown = DeserializeDouble(stream);
        return new StatusTickContext
        {
            Interval = interval,
            Countdown = countdown,
        };
    }
}
