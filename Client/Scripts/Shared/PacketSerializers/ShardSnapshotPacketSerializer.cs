using System.Collections.Immutable;
using Soteo.Gameplay;
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
    }

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
            SizeOfNullable(entity.Position) +
            SizeOfNullable(entity.Azimuth) +
            SizeOfStats(entity.Stats);
    }
    
    private int SizeOfStats(ImmutableDictionary<Stat, float> stats) =>
        sizeof(int) + stats.Count * (sizeof(Stat) + sizeof(float));

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
            SerializeInt(entity.Stats.Count, ref span);
            foreach ((Stat stat, float value) in entity.Stats)
            {
                SerializeEnum(stat, ref span);
                SerializeFloat(value, ref span);
            }
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
            Stats = dataFlags.HasFlag(EntitySnapshotDataFlags.Stats) ? DeserializeStats(ref span) : []
        };
    }
    
    public ImmutableDictionary<Stat, float> DeserializeStats(ref Span<byte> span)
    {
        var pairs = new KeyValuePair<Stat, float>[DeserializeInt(ref span)];
        for (int i = 0; i < pairs.Length; i++)
            pairs[i] = new KeyValuePair<Stat, float>(DeserializeEnum<Stat>(ref span), DeserializeFloat(ref span));
        return pairs.ToImmutableDictionary();
    }
}