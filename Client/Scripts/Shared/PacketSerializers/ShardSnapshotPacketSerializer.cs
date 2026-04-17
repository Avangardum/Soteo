using System.Collections.Immutable;
using Soteo.Gameplay;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;
using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class ShardSnapshotPacketSerializer : PacketSerializer<ShardSnapshotPacket>
{
    [Flags]
    private enum EntitySnapshotDataFlags : ushort
    {
        None = 0,
        Identity = 1,
        Position = 1 << 1,
        Azimuth = 1 << 2,
        Stats = 1 << 3,
        AbilityStates = 1 << 4,
        CurrentAbilitySlot = 1 << 5,
        CurrentAbilityRemainingUseTime = 1 << 6
    }
    
    private const int SizeOfAbilityState = sizeof(int) + sizeof(int) + sizeof(float);

    protected override int PacketSize(ShardSnapshotPacket packet)
    {
        return base.PacketSize(packet) + SizeOf(packet.Tick) + SizeOf(packet.Snapshot.Entities.Count) +
            packet.Snapshot.Entities.Sum(EntitySize);
    }
    
    private int EntitySize(EntitySnapshot entity)
    {
        return
            SizeOf(entity.Id) +
            sizeof(EntitySnapshotDataFlags) +
            SizeOfIgnoreNull(entity.Position) +
            SizeOfIgnoreNull(entity.Azimuth) +
            SizeOf(entity.Stats) +
            SizeOf(entity.AbilityStates, SizeOfAbilityState) +
            SizeOfIgnoreNull(entity.CurrentAbilitySlot) +
            SizeOfIgnoreNull(entity.CurrentAbilityRemainingUseTime);
    }

    protected override void SerializeInternal(ShardSnapshotPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeLong(packet.Tick, ref span);
        SerializeInt(packet.Snapshot.Entities.Count, ref span);
        
        foreach (EntitySnapshot entity in packet.Snapshot.Entities)
        {
            SerializeEntity(entity, ref span);
        }
    }
    
    private void SerializeEntity(EntitySnapshot entity, ref Span<byte> span)
    {
        SerializeGuid(entity.Id, ref span);
        Span<byte> dataFlagsSpan = SliceOff(sizeof(ushort), ref span);
        EntitySnapshotDataFlags dataFlags = EntitySnapshotDataFlags.None;
            
        if (entity.Position != null)
        {
            dataFlags |= EntitySnapshotDataFlags.Position;
            SerializeVector2(entity.Position.Value, ref span);
        }
        if (entity.Azimuth != null)
        {
            dataFlags |= EntitySnapshotDataFlags.Azimuth;
            SerializeFloat(entity.Azimuth.Value, ref span);
        }
        if (entity.Stats.Count > 0)
        {
            dataFlags |= EntitySnapshotDataFlags.Stats;
            SerializeDictionary(entity.Stats, SerializeEnum, SerializeFloat, ref span);
        }
        if (entity.AbilityStates.Count > 0)
        {
            dataFlags |= EntitySnapshotDataFlags.AbilityStates;
            SerializeDictionary(entity.AbilityStates, SerializeEnum, SerializeAbilityState, ref span);
        }
        if (entity.CurrentAbilitySlot != null)
        {
            dataFlags |= EntitySnapshotDataFlags.CurrentAbilitySlot;
            SerializeEnum(entity.CurrentAbilitySlot.Value, ref span);
        }
        if (entity.CurrentAbilityRemainingUseTime != null)
        {
            dataFlags |= EntitySnapshotDataFlags.CurrentAbilityRemainingUseTime;
            SerializeFloat(entity.CurrentAbilityRemainingUseTime.Value, ref span);
        }
            
        SerializeEnum(dataFlags, ref dataFlagsSpan);
    }

    protected override ShardSnapshotPacket DeserializeInternal(ref Span<byte> span)
    {
        var message = base.DeserializeInternal(ref span);
        message.Tick = DeserializeLong(ref span);
        var entities = new EntitySnapshot[DeserializeInt(ref span)];
        
        for (int i = 0; i < entities.Length; i++)
        {
            entities[i] = DeserializeEntity(ref span);
        }
        
        message.Snapshot = new ShardSnapshot { Entities = entities.ToImmutableList() };
        return message;
    }
    
    private EntitySnapshot DeserializeEntity(ref Span<byte> span)
    {
        var id = DeserializeGuid(ref span);
        var dataFlags = DeserializeEnum<EntitySnapshotDataFlags>(ref span);
        return new EntitySnapshot
        {
            Id = id,
            Position = dataFlags.HasFlag(EntitySnapshotDataFlags.Position) ? DeserializeVector2(ref span) : null,
            Azimuth = dataFlags.HasFlag(EntitySnapshotDataFlags.Azimuth) ? DeserializeFloat(ref span) : null,
            Stats = dataFlags.HasFlag(EntitySnapshotDataFlags.Stats) ?
                DeserializeDictionary(DeserializeEnum<Stat>, DeserializeFloat, ref span) : [],
            AbilityStates = dataFlags.HasFlag(EntitySnapshotDataFlags.AbilityStates) ?
                DeserializeDictionary(DeserializeEnum<AbilitySlot>, DeserializeAbilityState, ref span) : [],
            CurrentAbilitySlot = dataFlags.HasFlag(EntitySnapshotDataFlags.CurrentAbilitySlot) ?
                DeserializeEnum<AbilitySlot>(ref span) : null,
            CurrentAbilityRemainingUseTime = dataFlags.HasFlag(EntitySnapshotDataFlags.CurrentAbilityRemainingUseTime) ?
                DeserializeFloat(ref span) : null
        };
    }
    
    private void SerializeAbilityState(IReadOnlyAbilityState value, ref Span<byte> span)
    {
        SerializeInt(Ability.All.IndexOf(value.Ability), ref span);
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
}