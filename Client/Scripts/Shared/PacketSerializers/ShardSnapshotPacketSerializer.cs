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
    // todo too much boilerplate, refactor this
    
    private enum EntityKind : byte
    {
        Unit = 0,
        Projectile = 1
    }
    
    private const int SizeOfAbilityState = sizeof(int) + sizeof(int) + sizeof(float) + sizeof(float);
    private const int SizeOfAbilityUseProgress = sizeof(AbilitySlot) + sizeof(float) + sizeof(float);

    protected override int PacketSize(ShardSnapshotPacket packet)
    {
        return base.PacketSize(packet) + SizeOf(packet.Tick) + SizeOf(packet.Snapshot.Entities, EntitySize);
    }
    
    private int EntitySize(EntitySnapshot entity)
    {
        return entity switch
        {
            UnitSnapshot unit => UnitSize(unit),
            ProjectileSnapshot projectile => ProjectileSize(projectile)
        };
    }
    
    private int BaseEntitySize(EntitySnapshot entity)
    {
        return
            sizeof(EntityKind) +
            SizeOf(entity.Id) +
            SizeOf(entity.Position) +
            SizeOf(entity.Azimuth);
    }
    
    private int UnitSize(UnitSnapshot unit)
    {
        return
            BaseEntitySize(unit) +
            SizeOf(unit.IsMoving) +
            SizeOf(unit.Stats) +
            SizeOf(unit.AbilityStates, SizeOfAbilityState) +
            SizeOfNullableClass(unit.AbilityUseProgress, _ => SizeOfAbilityUseProgress);
    }
    
    private int ProjectileSize(ProjectileSnapshot projectile)
    {
        return BaseEntitySize(projectile) +
            SizeOf(projectile.Speed) +
            SizeOfAbilityContext(projectile.AbilityContext);
    }

    protected override void SerializeInternal(ShardSnapshotPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeLong(packet.Tick, ref span);
        SerializeList(packet.Snapshot.Entities, SerializeEntity, ref span);
    }
    
    private void SerializeEntity(EntitySnapshot entity, ref Span<byte> span)
    {
        switch (entity)
        {
            case UnitSnapshot unit:
                SerializeUnit(unit, ref span);
                break;
            case ProjectileSnapshot projectile:
                SerializeProjectile(projectile, ref span);
                break;
        }
    }
    
    private void SerializeBaseEntity(EntitySnapshot unit, ref Span<byte> span)
    {
        SerializeGuid(unit.Id, ref span);
        SerializeVector2(unit.Position, ref span);
        SerializeFloat(unit.Azimuth, ref span);
    }
    
    private void SerializeUnit(UnitSnapshot unit, ref Span<byte> span)
    {
        SerializeEnum(EntityKind.Unit, ref span);
        SerializeBaseEntity(unit, ref span);
        SerializeBool(unit.IsMoving, ref span);
        SerializeDictionary(unit.Stats, SerializeEnum, SerializeFloat, ref span);
        SerializeDictionary(unit.AbilityStates, SerializeEnum, SerializeAbilityState, ref span);
        SerializeNullableClass(unit.AbilityUseProgress, SerializeAbilityUseProgress, ref span);
    }
    
    private void SerializeProjectile(ProjectileSnapshot projectile, ref Span<byte> span)
    {
        SerializeEnum(EntityKind.Projectile, ref span);
        SerializeBaseEntity(projectile, ref span);
        SerializeFloat(projectile.Speed, ref span);
        SerializeAbilityContext(projectile.AbilityContext, ref span);
    }

    protected override ShardSnapshotPacket DeserializeInternal(ref Span<byte> span)
    {
        var message = base.DeserializeInternal(ref span);
        message.Tick = DeserializeLong(ref span);
        var entities = DeserializeList(DeserializeEntity, ref span).ToImmutableList();
        message.Snapshot = new ShardSnapshot { Entities = entities };
        return message;
    }
    
    private EntitySnapshot DeserializeEntity(ref Span<byte> span)
    {
        return DeserializeEnum<EntityKind>(ref span) switch
        {
            EntityKind.Unit => DeserializeUnit(ref span),
            EntityKind.Projectile => DeserializeProjectile(ref span)
        };
    }
    
    private UnitSnapshot DeserializeUnit(ref Span<byte> span)
    {
        return new UnitSnapshot
        {
            Id = DeserializeGuid(ref span),
            Position = DeserializeVector2(ref span),
            Azimuth = DeserializeFloat(ref span),
            IsMoving = DeserializeBool(ref span),
            Stats = DeserializeDictionary(DeserializeEnum<Stat>, DeserializeFloat, ref span),
            AbilityStates = DeserializeDictionary(DeserializeEnum<AbilitySlot>, DeserializeAbilityState, ref span),
            AbilityUseProgress = DeserializeNullableClass(DeserializeAbilityUseProgress, ref span)
        };
    }
    
    private ProjectileSnapshot DeserializeProjectile(ref Span<byte> span)
    {
        return new ProjectileSnapshot
        {
            Id = DeserializeGuid(ref span),
            Position = DeserializeVector2(ref span),
            Azimuth = DeserializeFloat(ref span),
            Speed = DeserializeFloat(ref span),
            AbilityContext = DeserializeAbilityContext(ref span)
        };
    }

    private void SerializeAbilityState(AbilityState value, ref Span<byte> span)
    {
        SerializeInt(value.Ability.Id, ref span);
        SerializeInt(value.Level, ref span);
        SerializeFloat(value.Cooldown, ref span);
        SerializeFloat(value.MaxCooldown, ref span);
    }
    
    private AbilityState DeserializeAbilityState(ref Span<byte> span)
    {
        return new AbilityState
        {
            Ability = Ability.All[DeserializeInt(ref span)],
            Level = DeserializeInt(ref span),
            Cooldown = DeserializeFloat(ref span),
            MaxCooldown = DeserializeFloat(ref span)
        };
    }
    
    private int SizeOfAbilityContext(AbilityContext.Deflated context)
    {
        return
            SizeOf(context.AbilityId) +
            SizeOf(context.Level) +
            SizeOf(context.UserId) +
            SizeOf(context.UserStats, SizeOf) +
            SizeOfNullableStruct(context.TargetPosition) +
            SizeOfNullableStruct(context.TargetUnitId) +
            SizeOfNullableStruct(context.TargetDirection) +
            SizeOfNullableStruct(context.TargetShardId);
    }
    
    private void SerializeAbilityContext(AbilityContext.Deflated context, ref Span<byte> span)
    {
        SerializeInt(context.AbilityId, ref span);
        SerializeInt(context.Level, ref span);
        SerializeGuid(context.UserId, ref span);
        SerializeDictionary(context.UserStats, SerializeEnum, SerializeFloat, ref span);
        SerializeNullableStruct(context.TargetPosition, SerializeVector2, ref span);
        SerializeNullableStruct(context.TargetUnitId, SerializeGuid, ref span);
        SerializeNullableStruct(context.TargetDirection, SerializeVector2, ref span);
        SerializeNullableStruct(context.TargetShardId, SerializeGuid, ref span);
    }
    
    private AbilityContext.Deflated DeserializeAbilityContext(ref Span<byte> span)
    {
        return new AbilityContext.Deflated
        {
            AbilityId = DeserializeInt(ref span),
            Level = DeserializeInt(ref span),
            UserId = DeserializeGuid(ref span),
            UserStats = DeserializeDictionary(DeserializeEnum<Stat>, DeserializeFloat, ref span),
            TargetPosition = DeserializeNullableStruct(DeserializeVector2, ref span),
            TargetUnitId = DeserializeNullableStruct(DeserializeGuid, ref span),
            TargetDirection = DeserializeNullableStruct(DeserializeVector2, ref span),
            TargetShardId = DeserializeNullableStruct(DeserializeGuid, ref span)
        };
    }
    
    private void SerializeAbilityUseProgress(AbilityUseProgress value, ref Span<byte> span)
    {
        SerializeEnum(value.Slot, ref span);
        SerializeFloat(value.ElapsedTime, ref span);
        SerializeFloat(value.RemainingTime, ref span);
    }
    
    private AbilityUseProgress DeserializeAbilityUseProgress(ref Span<byte> span)
    {
        return new AbilityUseProgress
        {
            Slot = DeserializeEnum<AbilitySlot>(ref span),
            ElapsedTime = DeserializeFloat(ref span),
            RemainingTime = DeserializeFloat(ref span)
        };
    }
}