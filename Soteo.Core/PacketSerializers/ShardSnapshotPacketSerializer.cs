using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class ShardSnapshotPacketSerializer(IGameplaySerializer gs) :
    PacketSerializer<ShardSnapshotPacket>
{
    protected override void SerializeInternal(ShardSnapshotPacket packet, Stream stream)
    {
        SerializeLong(packet.Snapshot.Tick, stream);
        SerializeIndexedDictionary(packet.Snapshot.Entities, SerializeEntity, stream);
    }
    
    protected override ShardSnapshotPacket DeserializeInternal(Stream stream)
    {
        return new ShardSnapshotPacket
        {
            Snapshot = new ShardSnapshot
            {
                Tick = DeserializeLong(stream),
                Entities = DeserializeIndexedDictionary(DeserializeEntity, it => it.Id, stream),
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
        return DeserializeEnum<EntityKind>(stream) switch
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
        SerializeGuid(entity.Id, stream);
        SerializeBool(entity.IsRemoved, stream);
        SerializeVector2(entity.Position, stream);
        SerializeDouble(entity.Azimuth, stream);
    }
    
    private void SerializeUnit(UnitSnapshot unit, Stream stream)
    {
        SerializeEnum(EntityKind.Unit, stream);
        SerializeBaseEntity(unit, stream);
        SerializeBool(unit.IsDead, stream);
        SerializeBool(unit.IsMoving, stream);
        SerializeDictionary(unit.Stats, SerializeEnum, SerializeDouble, stream);
        SerializeDictionary(unit.AbilitySlotStates, SerializeEnum, gs.SerializeAbilitySlotState, stream);
        SerializeNullableClass(unit.AbilityUseProgress, gs.SerializeAbilityUseProgress, stream);
        SerializeIndexedDictionary(unit.Statuses, SerializeStatusContext, stream);
    }
    
    private UnitSnapshot DeserializeUnit(Stream stream)
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
                DeserializeDictionary(DeserializeEnum<AbilitySlot>, gs.DeserializeAbilitySlotState, stream),
            AbilityUseProgress = DeserializeNullableClass(gs.DeserializeAbilityUseProgress, stream),
            Statuses = DeserializeIndexedDictionary(DeserializeStatusContext, it => it.Id, stream)
        };
    }
    
    private void SerializeUnitPuppet(UnitPuppetSnapshot unitPuppet, Stream stream)
    {
        SerializeEnum(EntityKind.UnitPuppet, stream);
        SerializeBaseEntity(unitPuppet, stream);
        SerializeBool(unitPuppet.IsDead, stream);
        SerializeBool(unitPuppet.IsMoving, stream);
        SerializeDictionary(unitPuppet.Stats, SerializeEnum, SerializeDouble, stream);
        SerializeDictionary(unitPuppet.AbilitySlotStates, SerializeEnum, gs.SerializeAbilitySlotState, stream);
        SerializeNullableClass(unitPuppet.AbilityUseProgress, gs.SerializeAbilityUseProgress, stream);
        SerializeIndexedDictionary(unitPuppet.Statuses, gs.SerializePuppetStatusContext, stream);
    }
    
    private UnitPuppetSnapshot DeserializeUnitPuppet(Stream stream)
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
                DeserializeDictionary(DeserializeEnum<AbilitySlot>, gs.DeserializeAbilitySlotState, stream),
            AbilityUseProgress = DeserializeNullableClass(gs.DeserializeAbilityUseProgress, stream),
            Statuses = DeserializeIndexedDictionary(gs.DeserializePuppetStatusContext, it => it.Id, stream)
        };
    }
    
    private void SerializeProjectile(ProjectileSnapshot projectile, Stream stream)
    {
        SerializeEnum(EntityKind.Projectile, stream);
        SerializeBaseEntity(projectile, stream);
        SerializeDouble(projectile.Speed, stream);
        SerializeAbilityContext(projectile.AbilityContext, stream);
        SerializeProjectileTarget(projectile.Target, stream);
    }
    
    private ProjectileSnapshot DeserializeProjectile(Stream stream)
    {
        return new ProjectileSnapshot
        {
            Id = DeserializeGuid(stream),
            IsRemoved = DeserializeBool(stream),
            Position = DeserializeVector2(stream),
            Azimuth = DeserializeDouble(stream),
            Speed = DeserializeDouble(stream),
            AbilityContext = DeserializeAbilityContext(stream),
            Target = DeserializeProjectileTarget(stream),
        };
    }
    
    private void SerializeProjectileTarget(DeflatedProjectileTarget value, Stream stream)
    {
        SerializeBool(value.IsUnit, stream);
        if (value.IsUnit)
            SerializeGuid(value.UnitId.Value, stream);
        else
            SerializeVector2(value.Position.Value, stream);
    }
    
    private DeflatedProjectileTarget DeserializeProjectileTarget(Stream stream)
    {
        bool isUnit = DeserializeBool(stream);
        if (isUnit)
            return DeserializeGuid(stream);
        else
            return DeserializeVector2(stream);
    }
    
    private void SerializeProjectilePuppet(ProjectilePuppetSnapshot projectilePuppet, Stream stream)
    {
        SerializeEnum(EntityKind.ProjectilePuppet, stream);
        SerializeBaseEntity(projectilePuppet, stream);
    }
    
    private ProjectilePuppetSnapshot DeserializeProjectilePuppet(Stream stream)
    {
        return new ProjectilePuppetSnapshot
        {
            Id = DeserializeGuid(stream),
            IsRemoved = DeserializeBool(stream),
            Position = DeserializeVector2(stream),
            Azimuth = DeserializeDouble(stream)
        };
    }
    
    private void SerializeAbilityContext(DeflatedAbilityContext context, Stream stream)
    {
        gs.SerializeAbility(context.Ability, stream);
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
            Ability = gs.DeserializeAbility(stream),
            Level = DeserializeInt(stream),
            UserId = DeserializeGuid(stream),
            UserStats = DeserializeDictionary(DeserializeEnum<Stat>, DeserializeDouble, stream),
            TargetPosition = DeserializeNullableStruct(DeserializeVector2, stream),
            TargetUnitId = DeserializeNullableStruct(DeserializeGuid, stream),
            TargetDirection = DeserializeNullableStruct(DeserializeVector2, stream),
            TargetShardId = DeserializeNullableStruct(DeserializeGuid, stream)
        };
    }
    
    private void SerializeStatusContext(DeflatedStatusContext value, Stream stream)
    {
        SerializeGuid(value.Id, stream);
        gs.SerializeStatus(value.Status, stream);
        SerializeNullableClass(value.AbilityContext, SerializeAbilityContext, stream);
        SerializeGuid(value.UnitId, stream);
        SerializeNullableStruct(value.SourceId, SerializeGuid, stream);
        SerializeNullableClass(value.Tick, SerializeStatusTickContext, stream);
        SerializeDouble(value.ElapsedTime, stream);
        SerializeDouble(value.DisplayElapsedTime, stream);
        SerializeDouble(value.RemainingTime, stream);
        SerializeLong(value.Ordinal, stream);
    }
    
    private DeflatedStatusContext DeserializeStatusContext(Stream stream)
    {
        return new DeflatedStatusContext
        {
            Id = DeserializeGuid(stream),
            Status = gs.DeserializeStatus(stream),
            AbilityContext = DeserializeNullableClass(DeserializeAbilityContext, stream),
            UnitId = DeserializeGuid(stream),
            SourceId = DeserializeNullableStruct(DeserializeGuid, stream),
            Tick = DeserializeNullableClass(DeserializeStatusTickContext, stream),
            ElapsedTime = DeserializeDouble(stream),
            DisplayElapsedTime = DeserializeDouble(stream),
            RemainingTime = DeserializeDouble(stream),
            Ordinal = DeserializeLong(stream),
        };
    }

    private void SerializeStatusTickContext(StatusTickContext value, Stream stream)
    {
        SerializeDouble(value.Interval, stream);
        SerializeDouble(value.Countdown, stream);
    }

    private StatusTickContext DeserializeStatusTickContext(Stream stream)
    {
        double interval = DeserializeDouble(stream);
        double countdown = DeserializeDouble(stream);
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
