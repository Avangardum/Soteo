using System.Collections.Immutable;
using Soteo.Gameplay;
using Soteo.Gameplay.Abilities;
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
    
    private const int SizeOfAbilityState = sizeof(int) + sizeof(int) + sizeof(float);

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
            SizeOf(unit.Stats) +
            SizeOf(unit.AbilityStates, SizeOfAbilityState) +
            SizeOf(unit.CurrentAbilitySlot) +
            SizeOf(unit.CurrentAbilityRemainingUseTime);
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
        SerializeDictionary(unit.Stats, SerializeEnum, SerializeFloat, ref span);
        SerializeDictionary(unit.AbilityStates, SerializeEnum, SerializeAbilityState, ref span);
        SerializeNullable(unit.CurrentAbilitySlot, SerializeEnum, ref span);
        SerializeNullable(unit.CurrentAbilityRemainingUseTime, SerializeFloat, ref span);
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
            Stats = DeserializeDictionary(DeserializeEnum<Stat>, DeserializeFloat, ref span),
            AbilityStates = DeserializeDictionary(DeserializeEnum<AbilitySlot>, DeserializeAbilityState, ref span),
            CurrentAbilitySlot = DeserializeNullable(DeserializeEnum<AbilitySlot>, ref span),
            CurrentAbilityRemainingUseTime = DeserializeNullable(DeserializeFloat, ref span)
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

    private void SerializeAbilityState(IReadOnlyAbilityState value, ref Span<byte> span)
    {
        SerializeInt(value.Ability.Id, ref span);
        SerializeInt(value.Level, ref span);
        SerializeFloat(value.Cooldown, ref span);
    }
    
    private IReadOnlyAbilityState DeserializeAbilityState(ref Span<byte> span)
    {
        Ability ability = Ability.All[DeserializeInt(ref span)];
        int level = DeserializeInt(ref span);
        float cooldown = DeserializeFloat(ref span);
        return new AbilityState(ability, level) { Cooldown = cooldown };
    }
    
    private int SizeOfAbilityContext(AbilityContext.Deflated context)
    {
        return
            SizeOf(context.AbilityId) +
            SizeOf(context.Level) +
            SizeOf(context.UserId) +
            SizeOf(context.UserStats, SizeOf) +
            SizeOf(context.TargetPosition) +
            SizeOf(context.TargetUnitId) +
            SizeOf(context.TargetDirection) +
            SizeOf(context.TargetShardId);
    }
    
    private void SerializeAbilityContext(AbilityContext.Deflated context, ref Span<byte> span)
    {
        SerializeInt(context.AbilityId, ref span);
        SerializeInt(context.Level, ref span);
        SerializeGuid(context.UserId, ref span);
        SerializeDictionary(context.UserStats, SerializeEnum, SerializeFloat, ref span);
        SerializeNullable(context.TargetPosition, SerializeVector2, ref span);
        SerializeNullable(context.TargetUnitId, SerializeGuid, ref span);
        SerializeNullable(context.TargetDirection, SerializeVector2, ref span);
        SerializeNullable(context.TargetShardId, SerializeGuid, ref span);
    }
    
    private AbilityContext.Deflated DeserializeAbilityContext(ref Span<byte> span)
    {
        return new AbilityContext.Deflated
        {
            AbilityId = DeserializeInt(ref span),
            Level = DeserializeInt(ref span),
            UserId = DeserializeGuid(ref span),
            UserStats = DeserializeDictionary(DeserializeEnum<Stat>, DeserializeFloat, ref span),
            TargetPosition = DeserializeNullable(DeserializeVector2, ref span),
            TargetUnitId = DeserializeNullable(DeserializeGuid, ref span),
            TargetDirection = DeserializeNullable(DeserializeVector2, ref span),
            TargetShardId = DeserializeNullable(DeserializeGuid, ref span)
        };
    }
}