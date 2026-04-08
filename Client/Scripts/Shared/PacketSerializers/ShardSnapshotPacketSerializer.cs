using System.Collections.Immutable;
using Soteo.Gameplay;
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
        Health = 1 << 3,
        Mana = 1 << 4,
        Experience = 1 << 5,
        Animation = 1 << 6,
        Cooldowns = 1 << 7,
        StatusEffects = 1 << 8,
        Inventory = 1 << 9
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
            SizeOfNullable(entity.Azimuth);
    }

    protected override void SerializeInternal(ShardSnapshotPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeLong(packet.Tick, ref span);
        SerializeInt(packet.Snapshot.Entities.Count, ref span);
        
        foreach (EntitySnapshot entity in packet.Snapshot.Entities)
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
            
            SerializeEnum(dataFlags, ref dataFlagsSpan);
        }
    }

    protected override ShardSnapshotPacket DeserializeInternal(ref Span<byte> span)
    {
        var message = base.DeserializeInternal(ref span);
        message.Tick = DeserializeLong(ref span);
        var entities = new EntitySnapshot[DeserializeInt(ref span)];
        
        for (int i = 0; i < entities.Length; i++)
        {
            var id = DeserializeGuid(ref span);
            var dataFlags = DeserializeEnum<EntitySnapshotDataFlags>(ref span);
            entities[i] = new EntitySnapshot
            {
                Id = id,
                Position = dataFlags.HasFlag(EntitySnapshotDataFlags.Position) ? DeserializeVector2(ref span) : null,
                Azimuth = dataFlags.HasFlag(EntitySnapshotDataFlags.Azimuth) ? DeserializeFloat(ref span) : null
            };
        }
        
        message.Snapshot = new ShardSnapshot { Entities = entities.ToImmutableList() };
        return message;
    }
}