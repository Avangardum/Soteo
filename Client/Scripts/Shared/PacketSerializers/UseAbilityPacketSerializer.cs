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
            SizeOf(packet.Command.TargetPosition) +
            SizeOf(packet.Command.TargetUnitId) +
            SizeOf(packet.Command.TargetDirection) + 
            SizeOf(packet.Command.TargetShardId); 
    }

    protected override void SerializeInternal(UseAbilityPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeEnum(packet.Command.Slot, ref span);
        SerializeBool(packet.Command.Repeat, ref span);
        
        SerializeNullable(packet.Command.TargetPosition, SerializeVector2, ref span);
        SerializeNullable(packet.Command.TargetUnitId, SerializeGuid, ref span);
        SerializeNullable(packet.Command.TargetDirection, SerializeVector2, ref span);
        SerializeNullable(packet.Command.TargetShardId, SerializeGuid, ref span);
    }

    protected override UseAbilityPacket DeserializeInternal(ref Span<byte> span)
    {
        var packet = base.DeserializeInternal(ref span);
        var slot = DeserializeEnum<AbilitySlot>(ref span);
        var repeat = DeserializeBool(ref span);
        
        Vector2? targetPosition = DeserializeNullable(DeserializeVector2, ref span);
        Guid? targetUnitId = DeserializeNullable(DeserializeGuid, ref span);
        Vector2? targetDirection = DeserializeNullable(DeserializeVector2, ref span);
        Guid? targetShardId = DeserializeNullable(DeserializeGuid, ref span);
        
        packet.Command =
            new UseAbilityCommand(slot, repeat, targetPosition, targetUnitId, targetDirection, targetShardId);
        return packet;
    }
}