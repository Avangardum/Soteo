using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared.PacketSerializers;
using static Soteo.Core.Shared.SerializationHelper;
using static Soteo.Core.Gameplay.GameplaySerializationHelper; 

namespace Soteo.Core.Gameplay.PacketSerializers;

public sealed class ShardSnapshotDeltaPacketSerializer : PacketSerializer<ShardSnapshotDeltaPacket>
{
    protected override void SerializeInternal(ShardSnapshotDeltaPacket packet, Stream stream)
    {
        SerializeLong(packet.Tick, stream);
        SerializeDouble(packet.ServerLoad, stream);
        SerializeIndexedDictionaryDelta(packet.SnapshotDelta.Entities, SerializeGuid, SerializeEntityDelta, stream);
    }

    protected override ShardSnapshotDeltaPacket DeserializeInternal(Stream stream)
    {
        return new ShardSnapshotDeltaPacket
        {
            Tick = DeserializeLong(stream),
            ServerLoad = DeserializeDouble(stream),
            SnapshotDelta = new ShardSnapshotDelta
            {
                Entities = DeserializeIndexedDictionaryDelta
                (
                    DeserializeGuid,
                    DeserializeEntityDelta,
                    it => it.Id,
                    stream
                ),
            },
        };
    }

    private void SerializeEntityDelta(EntitySnapshotDelta entity, Stream stream)
    {
        switch (entity)
        {
            case UnitPuppetSnapshotDelta unitPuppet:
                SerializeUnitPuppetDelta(unitPuppet, stream);
                break;
            case ProjectilePuppetSnapshotDelta projectilePuppet:
                SerializeProjectilePuppetDelta(projectilePuppet, stream);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private EntitySnapshotDelta DeserializeEntityDelta(Stream stream)
    {
        return DeserializeEnum<EntityKind>(stream) switch
        {
            EntityKind.UnitPuppet => DeserializeUnitPuppetDelta(stream),
            EntityKind.ProjectilePuppet => DeserializeProjectilePuppetDelta(stream),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private void SerializeBaseEntityDelta(EntitySnapshotDelta delta, Stream stream)
    {
        SerializeGuid(delta.Id, stream);
        SerializeDelta(delta.Position, SerializeVector2, stream);
        SerializeDelta(delta.Azimuth, SerializeDouble, stream);
    }

    private void SerializeUnitPuppetDelta(UnitPuppetSnapshotDelta delta, Stream stream)
    {
        SerializeEnum(EntityKind.UnitPuppet, stream);
        SerializeBaseEntityDelta(delta, stream);
        SerializeDelta(delta.IsDead, SerializeBool, stream);
        SerializeDelta(delta.IsMoving, SerializeBool, stream);
        SerializeDictionaryDelta(delta.Stats, SerializeEnum, SerializeDouble, stream);
        SerializeDictionaryDelta(delta.AbilitySlotStates, SerializeEnum, SerializeAbilitySlotState, stream);
        SerializeNullableClassDelta(delta.AbilityUseProgress, SerializeAbilityUseProgress, stream);
        SerializeDictionaryDelta(delta.Statuses, SerializeGuid, SerializePuppetStatusContext, stream);
    }

    private UnitPuppetSnapshotDelta DeserializeUnitPuppetDelta(Stream stream)
    {
        return new UnitPuppetSnapshotDelta
        {
            Id = DeserializeGuid(stream),
            Position = DeserializeDelta(DeserializeVector2, stream),
            Azimuth = DeserializeDelta(DeserializeDouble, stream),
            IsDead = DeserializeDelta(DeserializeBool, stream),
            IsMoving = DeserializeDelta(DeserializeBool, stream),
            Stats = DeserializeDictionaryDelta(DeserializeEnum<Stat>, DeserializeDouble, stream),
            AbilitySlotStates =
                DeserializeDictionaryDelta(DeserializeEnum<AbilitySlot>, DeserializeAbilitySlotState, stream),
            AbilityUseProgress = DeserializeNullableClassDelta(DeserializeAbilityUseProgress, stream),
            Statuses = DeserializeDictionaryDelta(DeserializeGuid, DeserializePuppetStatusContext, stream)
        };
    }

    private void SerializeProjectilePuppetDelta(ProjectilePuppetSnapshotDelta delta, Stream stream)
    {
        SerializeEnum(EntityKind.ProjectilePuppet, stream);
        SerializeBaseEntityDelta(delta, stream);
    }

    private ProjectilePuppetSnapshotDelta DeserializeProjectilePuppetDelta(Stream stream)
    {
        return new ProjectilePuppetSnapshotDelta
        {
            Id = DeserializeGuid(stream),
            Position = DeserializeDelta(DeserializeVector2, stream),
            Azimuth = DeserializeDelta(DeserializeDouble, stream)
        };
    }

    private void SerializeDelta<T>(Delta<T> delta, Serializer<T> serializer, Stream stream)
    {
        SerializeBool(delta.HasChanged, stream);
        if (delta.HasChanged)
            serializer(delta.NewValue, stream);
    }

    private Delta<T> DeserializeDelta<T>(Deserializer<T> deserializer, Stream stream)
    {
        return DeserializeBool(stream) ? deserializer(stream) : Delta<T>.Unchanged;
    }

    private void SerializeNullableClassDelta<T>(Delta<T?> delta, Serializer<T> serializer, Stream stream)
        where T : class
    {
        SerializeBool(delta.HasChanged, stream);
        if (delta.HasChanged)
            SerializeNullableClass(delta.NewValue, serializer, stream);
    }
    
    private Delta<T?> DeserializeNullableClassDelta<T>(Deserializer<T> deserializer, Stream stream)
        where T : class
    {
        return DeserializeBool(stream) ? DeserializeNullableClass(deserializer, stream) : Delta<T?>.Unchanged;
    }

    private void SerializeDictionaryDelta<TKey, TValue>
    (
        DictionaryDelta<TKey, TValue> delta,
        Serializer<TKey> serializeKey,
        Serializer<TValue> serializeValue,
        Stream stream
    ) where TKey : notnull
    {
        SerializeDictionary(delta.Changes, serializeKey, serializeValue, stream);
        SerializeList(delta.RemovedKeys, serializeKey, stream);
    }

    private DictionaryDelta<TKey, TValue> DeserializeDictionaryDelta<TKey, TValue>
    (
        Deserializer<TKey> deserializeKey,
        Deserializer<TValue> deserializeValue,
        Stream stream
    ) where TKey : notnull
    {
        var changes = DeserializeDictionary(deserializeKey, deserializeValue, stream);
        var removedKeys = DeserializeList(deserializeKey, stream);
        return new DictionaryDelta<TKey, TValue> { Changes = changes, RemovedKeys = removedKeys };
    }
    
    private void SerializeIndexedDictionaryDelta<TKey, TValue>
    (
        DictionaryDelta<TKey, TValue> delta,
        Serializer<TKey> serializeKey,
        Serializer<TValue> serializeValue,
        Stream stream
    ) where TKey : notnull
    {
        SerializeIndexedDictionary(delta.Changes, serializeValue, stream);
        SerializeList(delta.RemovedKeys, serializeKey, stream);
    }
    
    private DictionaryDelta<TKey, TValue> DeserializeIndexedDictionaryDelta<TKey, TValue>
    (
        Deserializer<TKey> deserializeKey,
        Deserializer<TValue> deserializeValue,
        Func<TValue, TKey> keySelector,
        Stream stream
    ) where TKey : notnull
    {
        var changes = DeserializeIndexedDictionary(deserializeValue, keySelector, stream);
        var removedKeys = DeserializeList(deserializeKey, stream);
        return new DictionaryDelta<TKey, TValue> { Changes = changes, RemovedKeys = removedKeys };
    }
    
    private enum EntityKind : byte
    {
        UnitPuppet,
        ProjectilePuppet
    }
}
