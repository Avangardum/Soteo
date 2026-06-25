using Soteo.Core.Delegates;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

public sealed class ShardSnapshotDeltaPacketSerializer(ISerializationHelper s) :
    PacketSerializer<ShardSnapshotDeltaPacket>(s)
{
    protected override void SerializeInternal(ShardSnapshotDeltaPacket packet, Stream stream)
    {
        s.SerializeDouble(packet.ServerLoad, stream);
        s.SerializeLong(packet.SnapshotDelta.Tick, stream);
        SerializeIndexedDictionaryDelta(packet.SnapshotDelta.Entities, s.SerializeGuid, SerializeEntityDelta, stream);
    }

    protected override ShardSnapshotDeltaPacket DeserializeInternal(Stream stream)
    {
        return new ShardSnapshotDeltaPacket
        {
            ServerLoad = s.DeserializeDouble(stream),
            SnapshotDelta = new ShardSnapshotDelta
            {
                Tick = s.DeserializeLong(stream),
                Entities = DeserializeIndexedDictionaryDelta
                (
                    s.DeserializeGuid,
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
        return s.DeserializeEnum<EntityKind>(stream) switch
        {
            EntityKind.UnitPuppet => DeserializeUnitPuppetDelta(stream),
            EntityKind.ProjectilePuppet => DeserializeProjectilePuppetDelta(stream),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private void SerializeBaseEntityDelta(EntitySnapshotDelta delta, Stream stream)
    {
        s.SerializeGuid(delta.Id, stream);
        SerializeDelta(delta.Position, s.SerializeVector2, stream);
        SerializeDelta(delta.Azimuth, s.SerializeDouble, stream);
    }

    private void SerializeUnitPuppetDelta(UnitPuppetSnapshotDelta delta, Stream stream)
    {
        s.SerializeEnum(EntityKind.UnitPuppet, stream);
        SerializeBaseEntityDelta(delta, stream);
        SerializeDelta(delta.IsDead, s.SerializeBool, stream);
        SerializeDelta(delta.IsMoving, s.SerializeBool, stream);
        SerializeDictionaryDelta(delta.Stats, s.SerializeEnum, s.SerializeDouble, stream);
        SerializeDictionaryDelta(delta.AbilitySlotStates, s.SerializeEnum, s.SerializeAbilitySlotState, stream);
        SerializeNullableClassDelta(delta.AbilityUseProgress, s.SerializeAbilityUseProgress, stream);
        SerializeDictionaryDelta(delta.Statuses, s.SerializeGuid, s.SerializePuppetStatusContext, stream);
    }

    private UnitPuppetSnapshotDelta DeserializeUnitPuppetDelta(Stream stream)
    {
        return new UnitPuppetSnapshotDelta
        {
            Id = s.DeserializeGuid(stream),
            Position = DeserializeDelta(s.DeserializeVector2, stream),
            Azimuth = DeserializeDelta(s.DeserializeDouble, stream),
            IsDead = DeserializeDelta(s.DeserializeBool, stream),
            IsMoving = DeserializeDelta(s.DeserializeBool, stream),
            Stats = DeserializeDictionaryDelta(s.DeserializeEnum<Stat>, s.DeserializeDouble, stream),
            AbilitySlotStates =
                DeserializeDictionaryDelta(s.DeserializeEnum<AbilitySlot>, s.DeserializeAbilitySlotState, stream),
            AbilityUseProgress = DeserializeNullableClassDelta(s.DeserializeAbilityUseProgress, stream),
            Statuses = DeserializeDictionaryDelta(s.DeserializeGuid, s.DeserializePuppetStatusContext, stream)
        };
    }

    private void SerializeProjectilePuppetDelta(ProjectilePuppetSnapshotDelta delta, Stream stream)
    {
        s.SerializeEnum(EntityKind.ProjectilePuppet, stream);
        SerializeBaseEntityDelta(delta, stream);
    }

    private ProjectilePuppetSnapshotDelta DeserializeProjectilePuppetDelta(Stream stream)
    {
        return new ProjectilePuppetSnapshotDelta
        {
            Id = s.DeserializeGuid(stream),
            Position = DeserializeDelta(s.DeserializeVector2, stream),
            Azimuth = DeserializeDelta(s.DeserializeDouble, stream)
        };
    }

    private void SerializeDelta<T>(Delta<T> delta, Serializer<T> serializer, Stream stream)
    {
        s.SerializeBool(delta.HasChanged, stream);
        if (delta.HasChanged)
            serializer(delta.NewValue, stream);
    }

    private Delta<T> DeserializeDelta<T>(Deserializer<T> deserializer, Stream stream)
    {
        return s.DeserializeBool(stream) ? deserializer(stream) : Delta<T>.Unchanged;
    }

    private void SerializeNullableClassDelta<T>(Delta<T?> delta, Serializer<T> serializer, Stream stream)
        where T : class
    {
        s.SerializeBool(delta.HasChanged, stream);
        if (delta.HasChanged)
            s.SerializeNullableClass(delta.NewValue, serializer, stream);
    }
    
    private Delta<T?> DeserializeNullableClassDelta<T>(Deserializer<T> deserializer, Stream stream)
        where T : class
    {
        return s.DeserializeBool(stream) ? s.DeserializeNullableClass(deserializer, stream) : Delta<T?>.Unchanged;
    }

    private void SerializeDictionaryDelta<TKey, TValue>
    (
        DictionaryDelta<TKey, TValue> delta,
        Serializer<TKey> serializeKey,
        Serializer<TValue> serializeValue,
        Stream stream
    ) where TKey : notnull
    {
        s.SerializeDictionary(delta.Changes, serializeKey, serializeValue, stream);
        s.SerializeList(delta.RemovedKeys, serializeKey, stream);
    }

    private DictionaryDelta<TKey, TValue> DeserializeDictionaryDelta<TKey, TValue>
    (
        Deserializer<TKey> deserializeKey,
        Deserializer<TValue> deserializeValue,
        Stream stream
    ) where TKey : notnull
    {
        var changes = s.DeserializeDictionary(deserializeKey, deserializeValue, stream);
        var removedKeys = s.DeserializeList(deserializeKey, stream);
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
        s.SerializeIndexedDictionary(delta.Changes, serializeValue, stream);
        s.SerializeList(delta.RemovedKeys, serializeKey, stream);
    }
    
    private DictionaryDelta<TKey, TValue> DeserializeIndexedDictionaryDelta<TKey, TValue>
    (
        Deserializer<TKey> deserializeKey,
        Deserializer<TValue> deserializeValue,
        Func<TValue, TKey> keySelector,
        Stream stream
    ) where TKey : notnull
    {
        var changes = s.DeserializeIndexedDictionary(deserializeValue, keySelector, stream);
        var removedKeys = s.DeserializeList(deserializeKey, stream);
        return new DictionaryDelta<TKey, TValue> { Changes = changes, RemovedKeys = removedKeys };
    }
}
