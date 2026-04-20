using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Enums;
using Soteo.Shared.Enums;
using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class UseAbilityPacketSerializer : PacketSerializer<UseAbilityPacket>
{
    protected override int PacketSize(UseAbilityPacket packet)
    {
        return base.PacketSize(packet) +
            SizeOf(packet.Command.Slot) +
            SizeOf(packet.Command.Repeat) +
            sizeof(AbilityTargetFlags) +
            SizeOfIgnoreNull(packet.Command.TargetPosition) +
            SizeOfIgnoreNull(packet.Command.TargetUnitId) +
            SizeOfIgnoreNull(packet.Command.TargetDirection) + 
            SizeOfIgnoreNull(packet.Command.TargetShardId); 
    }

    protected override void SerializeInternal(UseAbilityPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeEnum(packet.Command.Slot, ref span);
        SerializeBool(packet.Command.Repeat, ref span);
        Span<byte> targetFlagsSpan = SliceOff(sizeof(AbilityTargetFlags), ref span);
        var targetFlags = AbilityTargetFlags.None;
        
        if (packet.Command.TargetPosition != null)
        {
            targetFlags |= AbilityTargetFlags.Position;
            SerializeVector2(packet.Command.TargetPosition.Value, ref span);
        }
        if (packet.Command.TargetUnitId != null)
        {
            targetFlags |= AbilityTargetFlags.Unit;
            SerializeGuid(packet.Command.TargetUnitId.Value, ref span);
        }
        if (packet.Command.TargetDirection != null)
        {
            targetFlags |= AbilityTargetFlags.HasDirection;
            SerializeVector2(packet.Command.TargetDirection.Value, ref span);
        }
        if (packet.Command.TargetShardId != null)
        {
            targetFlags |= AbilityTargetFlags.HasShard;
            SerializeGuid(packet.Command.TargetShardId.Value, ref span);
        }
        
        SerializeEnum(targetFlags, ref targetFlagsSpan);
    }

    protected override UseAbilityPacket DeserializeInternal(ref Span<byte> span)
    {
        var packet = base.DeserializeInternal(ref span);
        var slot = DeserializeEnum<AbilitySlot>(ref span);
        var repeat = DeserializeBool(ref span);
        var targetFlags = DeserializeEnum<AbilityTargetFlags>(ref span);
        
        Vector2? targetPosition =
            targetFlags.HasFlag(AbilityTargetFlags.Position) ? DeserializeVector2(ref span) : null;
        Guid? targetUnitId =
            targetFlags.HasFlag(AbilityTargetFlags.Unit) ? DeserializeGuid(ref span) : null;
        Vector2? targetDirection =
            targetFlags.HasFlag(AbilityTargetFlags.HasDirection) ? DeserializeVector2(ref span) : null;
        Guid? targetShardId =
            targetFlags.HasFlag(AbilityTargetFlags.HasShard) ? DeserializeGuid(ref span) : null;
        
        packet.Command =
            new UseAbilityCommand(slot, repeat, targetPosition, targetUnitId, targetDirection, targetShardId);
        return packet;
    }
}