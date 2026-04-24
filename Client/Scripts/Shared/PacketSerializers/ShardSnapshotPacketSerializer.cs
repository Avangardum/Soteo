using System.Collections.Immutable;
using Soteo.Gameplay;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;
using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class ShardSnapshotPacketSerializer : PacketSerializer<ShardSnapshotPacket>
{
    private const int SizeOfAbilityState = sizeof(int) + sizeof(int) + sizeof(float);

    protected override int PacketSize(ShardSnapshotPacket packet)
    {
        return base.PacketSize(packet) + SizeOf(packet.Tick) + SizeOf(packet.Snapshot.Entities, EntitySize);
    }
    
    private int EntitySize(EntitySnapshot entity)
    {
        return
            SizeOf(entity.Id) +
            SizeOf(entity.Position) +
            SizeOf(entity.Azimuth) +
            SizeOf(entity.Stats) +
            SizeOf(entity.AbilityStates, SizeOfAbilityState) +
            SizeOf(entity.CurrentAbilitySlot) +
            SizeOf(entity.CurrentAbilityRemainingUseTime);
    }

    protected override void SerializeInternal(ShardSnapshotPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeLong(packet.Tick, ref span);
        SerializeList(packet.Snapshot.Entities, SerializeEntity, ref span);
    }
    
    private void SerializeEntity(EntitySnapshot entity, ref Span<byte> span)
    {
        SerializeGuid(entity.Id, ref span);
        SerializeNullable(entity.Position, SerializeVector2, ref span);
        SerializeNullable(entity.Azimuth, SerializeFloat, ref span);
        SerializeDictionary(entity.Stats, SerializeEnum, SerializeFloat, ref span);
        SerializeDictionary(entity.AbilityStates, SerializeEnum, SerializeAbilityState, ref span);
        SerializeNullable(entity.CurrentAbilitySlot, SerializeEnum, ref span);
        SerializeNullable(entity.CurrentAbilityRemainingUseTime, SerializeFloat, ref span);
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
        return new EntitySnapshot
        {
            Id = DeserializeGuid(ref span),
            Position = DeserializeNullable(DeserializeVector2, ref span),
            Azimuth = DeserializeNullable(DeserializeFloat, ref span),
            Stats = DeserializeDictionary(DeserializeEnum<Stat>, DeserializeFloat, ref span),
            AbilityStates = DeserializeDictionary(DeserializeEnum<AbilitySlot>, DeserializeAbilityState, ref span),
            CurrentAbilitySlot = DeserializeNullable(DeserializeEnum<AbilitySlot>, ref span),
            CurrentAbilityRemainingUseTime = DeserializeNullable(DeserializeFloat, ref span)
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
}