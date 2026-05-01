using System.Collections.Immutable;
using Soteo.Gameplay;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;
using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class ShardSnapshotPacketSerializer : PacketSerializer<ShardSnapshotPacket>
{
    private enum EntityKind : byte
    {
        Unit = 0,
        Projectile = 1
    }
    
    protected override void SerializeInternal(ShardSnapshotPacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeLong(packet.Tick, stream);
        SerializeList(packet.Snapshot.Entities, SerializeEntity, stream);
    }
    
    private void SerializeEntity(EntitySnapshot entity, Stream stream)
    {
        switch (entity)
        {
            case UnitSnapshot unit:
                SerializeUnit(unit, stream);
                break;
            case ProjectileSnapshot projectile:
                SerializeProjectile(projectile, stream);
                break;
        }
    }
    
    private void SerializeBaseEntity(EntitySnapshot unit, Stream stream)
    {
        SerializeGuid(unit.Id, stream);
        SerializeVector2(unit.Position, stream);
        SerializeFloat(unit.Azimuth, stream);
    }
    
    private void SerializeUnit(UnitSnapshot unit, Stream stream)
    {
        SerializeEnum(EntityKind.Unit, stream);
        SerializeBaseEntity(unit, stream);
        SerializeBool(unit.IsMoving, stream);
        SerializeDictionary(unit.Stats, SerializeEnum, SerializeDouble, stream);
        SerializeDictionary(unit.AbilityStates, SerializeEnum, SerializeAbilityState, stream);
        SerializeNullableClass(unit.AbilityUseProgress, SerializeAbilityUseProgress, stream);
        SerializeIndexedDictionary(unit.Statuses, SerializeStatusContext, stream);
    }
    
    private void SerializeProjectile(ProjectileSnapshot projectile, Stream stream)
    {
        SerializeEnum(EntityKind.Projectile, stream);
        SerializeBaseEntity(projectile, stream);
        SerializeFloat(projectile.Speed, stream);
        SerializeAbilityContext(projectile.AbilityContext, stream);
    }

    protected override ShardSnapshotPacket DeserializeInternal(Stream stream)
    {
        var message = base.DeserializeInternal(stream);
        message.Tick = DeserializeLong(stream);
        var entities = DeserializeList(DeserializeEntity, stream).ToImmutableList();
        message.Snapshot = new ShardSnapshot { Entities = entities };
        return message;
    }
    
    private EntitySnapshot DeserializeEntity(Stream stream)
    {
        return DeserializeEnum<EntityKind>(stream) switch
        {
            EntityKind.Unit => DeserializeUnit(stream),
            EntityKind.Projectile => DeserializeProjectile(stream)
        };
    }
    
    private UnitSnapshot DeserializeUnit(Stream stream)
    {
        return new UnitSnapshot
        {
            Id = DeserializeGuid(stream),
            Position = DeserializeVector2(stream),
            Azimuth = DeserializeFloat(stream),
            IsMoving = DeserializeBool(stream),
            Stats = DeserializeDictionary(DeserializeEnum<Stat>, DeserializeDouble, stream),
            AbilityStates = DeserializeDictionary(DeserializeEnum<AbilitySlot>, DeserializeAbilityState, stream),
            AbilityUseProgress = DeserializeNullableClass(DeserializeAbilityUseProgress, stream),
            Statuses = DeserializeIndexedDictionary(DeserializeStatusContext, it => it.Id, stream)
        };
    }
    
    private ProjectileSnapshot DeserializeProjectile(Stream stream)
    {
        return new ProjectileSnapshot
        {
            Id = DeserializeGuid(stream),
            Position = DeserializeVector2(stream),
            Azimuth = DeserializeFloat(stream),
            Speed = DeserializeFloat(stream),
            AbilityContext = DeserializeAbilityContext(stream)
        };
    }

    private void SerializeAbilityState(AbilityState value, Stream stream)
    {
        SerializeInt(value.Ability.Id, stream);
        SerializeInt(value.Level, stream);
        SerializeFloat(value.Cooldown, stream);
        SerializeFloat(value.MaxCooldown, stream);
    }
    
    private AbilityState DeserializeAbilityState(Stream stream)
    {
        return new AbilityState
        {
            Ability = Ability.All[DeserializeInt(stream)],
            Level = DeserializeInt(stream),
            Cooldown = DeserializeFloat(stream),
            MaxCooldown = DeserializeFloat(stream)
        };
    }
    
    private void SerializeAbilityContext(DeflatedAbilityContext context, Stream stream)
    {
        SerializeInt(context.AbilityId, stream);
        SerializeInt(context.Level, stream);
        SerializeGuid(context.UserId, stream);
        SerializeDictionary(context.UserStats, SerializeEnum, SerializeDouble, stream);
        SerializeNullableStruct(context.TargetPosition, SerializeVector2, stream);
        SerializeNullableStruct(context.TargetUnitId, SerializeGuid, stream);
        SerializeNullableStruct(context.TargetDirection, SerializeVector2, stream);
        SerializeNullableStruct(context.TargetShardId, SerializeGuid, stream);
    }
    
    private DeflatedAbilityContext DeserializeAbilityContext(Stream stream)
    {
        return new DeflatedAbilityContext
        {
            AbilityId = DeserializeInt(stream),
            Level = DeserializeInt(stream),
            UserId = DeserializeGuid(stream),
            UserStats = DeserializeDictionary(DeserializeEnum<Stat>, DeserializeDouble, stream),
            TargetPosition = DeserializeNullableStruct(DeserializeVector2, stream),
            TargetUnitId = DeserializeNullableStruct(DeserializeGuid, stream),
            TargetDirection = DeserializeNullableStruct(DeserializeVector2, stream),
            TargetShardId = DeserializeNullableStruct(DeserializeGuid, stream)
        };
    }
    
    private void SerializeAbilityUseProgress(AbilityUseProgress value, Stream stream)
    {
        SerializeEnum(value.Slot, stream);
        SerializeFloat(value.ElapsedTime, stream);
        SerializeFloat(value.RemainingTime, stream);
    }
    
    private AbilityUseProgress DeserializeAbilityUseProgress(Stream stream)
    {
        return new AbilityUseProgress
        {
            Slot = DeserializeEnum<AbilitySlot>(stream),
            ElapsedTime = DeserializeFloat(stream),
            RemainingTime = DeserializeFloat(stream)
        };
    }
    
    private void SerializeStatusContext(DeflatedStatusContext value, Stream stream)
    {
        SerializeGuid(value.Id, stream);
        SerializeInt(value.StatusId, stream);
        SerializeNullableClass(value.AbilityContext, SerializeAbilityContext, stream);
        SerializeGuid(value.UnitId, stream);
        SerializeNullableStruct(value.SourceId, SerializeGuid, stream);
        SerializeDouble(value.TickCountdown, stream);
        SerializeDouble(value.DisplayElapsedTime, stream);
        SerializeDouble(value.RemainingTime, stream);
        SerializeDouble(value.TickInterval, stream);
    }
    
    private DeflatedStatusContext DeserializeStatusContext(Stream stream)
    {
        return new DeflatedStatusContext
        {
            Id = DeserializeGuid(stream),
            StatusId = DeserializeInt(stream),
            AbilityContext = DeserializeNullableClass(DeserializeAbilityContext, stream),
            UnitId = DeserializeGuid(stream),
            SourceId = DeserializeNullableStruct(DeserializeGuid, stream),
            TickCountdown = DeserializeDouble(stream),
            DisplayElapsedTime = DeserializeDouble(stream),
            RemainingTime = DeserializeDouble(stream),
            TickInterval = DeserializeDouble(stream)
        };
    }
}