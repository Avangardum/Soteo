using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Enums;
using Soteo.Shared.Enums;
using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class UseAbilityPacketSerializer : PacketSerializer<UseAbilityPacket>
{
    // todo don't use CanTarget
    
    protected override int PacketSize(UseAbilityPacket packet)
    {
        return base.PacketSize(packet) +
            SizeOf(packet.Command.Slot) +
            SizeOf(packet.Command.Repeat) +
            sizeof(CanTarget) +
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
        Span<byte> targetFlagsSpan = SliceOff(sizeof(CanTarget), ref span);
        var targetFlags = CanTarget.Invalid;
        
        if (packet.Command.TargetPosition != null)
        {
            targetFlags |= CanTarget.Position;
            SerializeVector2(packet.Command.TargetPosition.Value, ref span);
        }
        if (packet.Command.TargetUnitId != null)
        {
            targetFlags |= CanTarget.Character;
            SerializeGuid(packet.Command.TargetUnitId.Value, ref span);
        }
        if (packet.Command.TargetDirection != null)
        {
            targetFlags |= CanTarget.WithDirection;
            SerializeVector2(packet.Command.TargetDirection.Value, ref span);
        }
        if (packet.Command.TargetShardId != null)
        {
            targetFlags |= CanTarget.WithShard;
            SerializeGuid(packet.Command.TargetShardId.Value, ref span);
        }
        
        SerializeEnum(targetFlags, ref targetFlagsSpan);
    }

    protected override UseAbilityPacket DeserializeInternal(ref Span<byte> span)
    {
        var packet = base.DeserializeInternal(ref span);
        var slot = DeserializeEnum<AbilitySlot>(ref span);
        var repeat = DeserializeBool(ref span);
        var targetFlags = DeserializeEnum<CanTarget>(ref span);
        
        Vector2? targetPosition =
            targetFlags.HasFlag(CanTarget.Position) ? DeserializeVector2(ref span) : null;
        Guid? targetUnitId =
            targetFlags.HasFlag(CanTarget.Character) ? DeserializeGuid(ref span) : null;
        Vector2? targetDirection =
            targetFlags.HasFlag(CanTarget.WithDirection) ? DeserializeVector2(ref span) : null;
        Guid? targetShardId =
            targetFlags.HasFlag(CanTarget.WithShard) ? DeserializeGuid(ref span) : null;
        
        packet.Command =
            new UseAbilityCommand(slot, repeat, targetPosition, targetUnitId, targetDirection, targetShardId);
        return packet;
    }
}