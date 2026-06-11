using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketSerializers;

public sealed class ShardSnapshotPacketSerializer(ISerializationHelper s) : PacketSerializer<ShardSnapshotPacket>(s)
{
    protected override void SerializeInternal(ShardSnapshotPacket packet, Stream stream)
    {
        s.SerializeLong(packet.Snapshot.Tick, stream);
        s.SerializeIndexedDictionary(packet.Snapshot.Entities, SerializeEntity, stream);
    }
    
    protected override ShardSnapshotPacket DeserializeInternal(Stream stream)
    {
        return new ShardSnapshotPacket
        {
            Snapshot = new ShardSnapshot
            {
                Tick = s.DeserializeLong(stream),
                Entities = s.DeserializeIndexedDictionary(DeserializeEntity, it => it.Id, stream),
            },
        };
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
            case UnitPuppetSnapshot unitPuppet:
                SerializeUnitPuppet(unitPuppet, stream);
                break;
            case ProjectilePuppetSnapshot projectilePuppet:
                SerializeProjectilePuppet(projectilePuppet, stream);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private EntitySnapshot DeserializeEntity(Stream stream)
    {
        return s.DeserializeEnum<EntityKind>(stream) switch
        {
            EntityKind.Unit => DeserializeUnit(stream),
            EntityKind.Projectile => DeserializeProjectile(stream),
            EntityKind.UnitPuppet => DeserializeUnitPuppet(stream),
            EntityKind.ProjectilePuppet => DeserializeProjectilePuppet(stream),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
    
    private void SerializeBaseEntity(EntitySnapshot entity, Stream stream)
    {
        s.SerializeGuid(entity.Id, stream);
        s.SerializeBool(entity.IsRemoved, stream);
        s. SerializeVector2(entity.Position, stream);
        s.SerializeDouble(entity.Azimuth, stream);
    }
    
    private void SerializeUnit(UnitSnapshot unit, Stream stream)
    {
        s.SerializeEnum(EntityKind.Unit, stream);
        SerializeBaseEntity(unit, stream);
        s.SerializeBool(unit.IsDead, stream);
        s.SerializeBool(unit.IsMoving, stream);
        s.SerializeDictionary(unit.Stats, s.SerializeEnum, s.SerializeDouble, stream);
        s.SerializeDictionary(unit.AbilitySlotStates, s.SerializeEnum, s.SerializeAbilitySlotState, stream);
        s.SerializeNullableClass(unit.AbilityUseProgress, s.SerializeAbilityUseProgress, stream);
        s.SerializeIndexedDictionary(unit.Statuses, SerializeStatusContext, stream);
    }
    
    private UnitSnapshot DeserializeUnit(Stream stream)
    {
        return new UnitSnapshot
        {
            Id = s.DeserializeGuid(stream),
            IsRemoved = s.DeserializeBool(stream),
            Position = s.DeserializeVector2(stream),
            Azimuth = s.DeserializeDouble(stream),
            IsDead = s.DeserializeBool(stream),
            IsMoving = s.DeserializeBool(stream),
            Stats = s.DeserializeDictionary(s.DeserializeEnum<Stat>, s.DeserializeDouble, stream),
            AbilitySlotStates =
                s.DeserializeDictionary(s.DeserializeEnum<AbilitySlot>, s.DeserializeAbilitySlotState, stream),
            AbilityUseProgress = s.DeserializeNullableClass(s.DeserializeAbilityUseProgress, stream),
            Statuses = s.DeserializeIndexedDictionary(DeserializeStatusContext, it => it.Id, stream)
        };
    }
    
    private void SerializeUnitPuppet(UnitPuppetSnapshot unitPuppet, Stream stream)
    {
        s.SerializeEnum(EntityKind.UnitPuppet, stream);
        SerializeBaseEntity(unitPuppet, stream);
        s.SerializeBool(unitPuppet.IsDead, stream);
        s.SerializeBool(unitPuppet.IsMoving, stream);
        s.SerializeDictionary(unitPuppet.Stats, s.SerializeEnum, s.SerializeDouble, stream);
        s.SerializeDictionary(unitPuppet.AbilitySlotStates, s.SerializeEnum, s.SerializeAbilitySlotState, stream);
        s.SerializeNullableClass(unitPuppet.AbilityUseProgress, s.SerializeAbilityUseProgress, stream);
        s.SerializeIndexedDictionary(unitPuppet.Statuses, s.SerializePuppetStatusContext, stream);
    }
    
    private UnitPuppetSnapshot DeserializeUnitPuppet(Stream stream)
    {
        return new UnitPuppetSnapshot
        {
            Id = s.DeserializeGuid(stream),
            IsRemoved = s.DeserializeBool(stream),
            Position = s.DeserializeVector2(stream),
            Azimuth = s.DeserializeDouble(stream),
            IsDead = s.DeserializeBool(stream),
            IsMoving = s.DeserializeBool(stream),
            Stats = s.DeserializeDictionary(s.DeserializeEnum<Stat>, s.DeserializeDouble, stream),
            AbilitySlotStates =
                s.DeserializeDictionary(s.DeserializeEnum<AbilitySlot>, s.DeserializeAbilitySlotState, stream),
            AbilityUseProgress = s.DeserializeNullableClass(s.DeserializeAbilityUseProgress, stream),
            Statuses = s.DeserializeIndexedDictionary(s.DeserializePuppetStatusContext, it => it.Id, stream)
        };
    }
    
    private void SerializeProjectile(ProjectileSnapshot projectile, Stream stream)
    {
        s.SerializeEnum(EntityKind.Projectile, stream);
        SerializeBaseEntity(projectile, stream);
        s.SerializeDouble(projectile.Speed, stream);
        SerializeAbilityContext(projectile.AbilityContext, stream);
        SerializeProjectileTarget(projectile.Target, stream);
    }
    
    private ProjectileSnapshot DeserializeProjectile(Stream stream)
    {
        return new ProjectileSnapshot
        {
            Id = s.DeserializeGuid(stream),
            IsRemoved = s.DeserializeBool(stream),
            Position = s.DeserializeVector2(stream),
            Azimuth = s.DeserializeDouble(stream),
            Speed = s.DeserializeDouble(stream),
            AbilityContext = DeserializeAbilityContext(stream),
            Target = DeserializeProjectileTarget(stream),
        };
    }
    
    private void SerializeProjectileTarget(DeflatedProjectileTarget value, Stream stream)
    {
        s.SerializeBool(value.IsUnit, stream);
        if (value.IsUnit)
            s.SerializeGuid(value.UnitId.Value, stream);
        else
            s.SerializeVector2(value.Position.Value, stream);
    }
    
    private DeflatedProjectileTarget DeserializeProjectileTarget(Stream stream)
    {
        bool isUnit = s.DeserializeBool(stream);
        if (isUnit)
            return s.DeserializeGuid(stream);
        else
            return s.DeserializeVector2(stream);
    }
    
    private void SerializeProjectilePuppet(ProjectilePuppetSnapshot projectilePuppet, Stream stream)
    {
        s.SerializeEnum(EntityKind.ProjectilePuppet, stream);
        SerializeBaseEntity(projectilePuppet, stream);
    }
    
    private ProjectilePuppetSnapshot DeserializeProjectilePuppet(Stream stream)
    {
        return new ProjectilePuppetSnapshot
        {
            Id = s.DeserializeGuid(stream),
            IsRemoved = s.DeserializeBool(stream),
            Position = s.DeserializeVector2(stream),
            Azimuth = s.DeserializeDouble(stream)
        };
    }
    
    private void SerializeAbilityContext(DeflatedAbilityContext context, Stream stream)
    {
        s.SerializeAbility(context.Ability, stream);
        s.SerializeInt(context.Level, stream);
        s.SerializeGuid(context.UserId, stream);
        s.SerializeDictionary(context.UserStats, s.SerializeEnum, s.SerializeDouble, stream);
        s.SerializeNullableStruct(context.TargetPosition, s.SerializeVector2, stream);
        s.SerializeNullableStruct(context.TargetUnitId, s.SerializeGuid, stream);
        s.SerializeNullableStruct(context.TargetDirection, s.SerializeVector2, stream);
        s.SerializeNullableStruct(context.TargetShardId, s.SerializeGuid, stream);
    }
    
    private DeflatedAbilityContext DeserializeAbilityContext(Stream stream)
    {
        return new DeflatedAbilityContext
        {
            Ability = s.DeserializeAbility(stream),
            Level = s.DeserializeInt(stream),
            UserId = s.DeserializeGuid(stream),
            UserStats = s.DeserializeDictionary(s.DeserializeEnum<Stat>, s.DeserializeDouble, stream),
            TargetPosition = s.DeserializeNullableStruct(s.DeserializeVector2, stream),
            TargetUnitId = s.DeserializeNullableStruct(s.DeserializeGuid, stream),
            TargetDirection = s.DeserializeNullableStruct(s.DeserializeVector2, stream),
            TargetShardId = s.DeserializeNullableStruct(s.DeserializeGuid, stream)
        };
    }
    
    private void SerializeStatusContext(DeflatedStatusContext value, Stream stream)
    {
        s.SerializeGuid(value.Id, stream);
        s.SerializeStatus(value.Status, stream);
        s.SerializeNullableClass(value.AbilityContext, SerializeAbilityContext, stream);
        s.SerializeGuid(value.UnitId, stream);
        s.SerializeNullableStruct(value.SourceId, s.SerializeGuid, stream);
        s.SerializeNullableClass(value.Tick, SerializeStatusTickContext, stream);
        s.SerializeDouble(value.ElapsedTime, stream);
        s.SerializeDouble(value.DisplayElapsedTime, stream);
        s.SerializeDouble(value.RemainingTime, stream);
        s.SerializeLong(value.Ordinal, stream);
    }
    
    private DeflatedStatusContext DeserializeStatusContext(Stream stream)
    {
        return new DeflatedStatusContext
        {
            Id = s.DeserializeGuid(stream),
            Status = s.DeserializeStatus(stream),
            AbilityContext = s.DeserializeNullableClass(DeserializeAbilityContext, stream),
            UnitId = s.DeserializeGuid(stream),
            SourceId = s.DeserializeNullableStruct(s.DeserializeGuid, stream),
            Tick = s.DeserializeNullableClass(DeserializeStatusTickContext, stream),
            ElapsedTime = s.DeserializeDouble(stream),
            DisplayElapsedTime = s.DeserializeDouble(stream),
            RemainingTime = s.DeserializeDouble(stream),
            Ordinal = s.DeserializeLong(stream),
        };
    }

    private void SerializeStatusTickContext(StatusTickContext value, Stream stream)
    {
        s.SerializeDouble(value.Interval, stream);
        s.SerializeDouble(value.Countdown, stream);
    }

    private StatusTickContext DeserializeStatusTickContext(Stream stream)
    {
        double interval = s.DeserializeDouble(stream);
        double countdown = s.DeserializeDouble(stream);
        return new StatusTickContext
        {
            Interval = interval,
            Countdown = countdown,
        };
    }
    
    private enum EntityKind : byte
    {
        Unit,
        Projectile,
        UnitPuppet,
        ProjectilePuppet
    }
}
