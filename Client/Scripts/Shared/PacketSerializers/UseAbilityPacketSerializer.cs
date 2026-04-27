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
            SizeOfNullableStruct(packet.Command.TargetPosition) +
            SizeOfNullableStruct(packet.Command.TargetUnitId) +
            SizeOfNullableStruct(packet.Command.TargetDirection) + 
            SizeOfNullableStruct(packet.Command.TargetShardId); 
    }

    protected override void SerializeInternal(UseAbilityPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeEnum(packet.Command.Slot, ref span);
        SerializeBool(packet.Command.Repeat, ref span);
        
        SerializeNullableStruct(packet.Command.TargetPosition, SerializeVector2, ref span);
        SerializeNullableStruct(packet.Command.TargetUnitId, SerializeGuid, ref span);
        SerializeNullableStruct(packet.Command.TargetDirection, SerializeVector2, ref span);
        SerializeNullableStruct(packet.Command.TargetShardId, SerializeGuid, ref span);
    }

    protected override UseAbilityPacket DeserializeInternal(ref Span<byte> span)
    {
        var packet = base.DeserializeInternal(ref span);
        var slot = DeserializeEnum<AbilitySlot>(ref span);
        var repeat = DeserializeBool(ref span);
        
        Vector2? targetPosition = DeserializeNullableStruct(DeserializeVector2, ref span);
        Guid? targetUnitId = DeserializeNullableStruct(DeserializeGuid, ref span);
        Vector2? targetDirection = DeserializeNullableStruct(DeserializeVector2, ref span);
        Guid? targetShardId = DeserializeNullableStruct(DeserializeGuid, ref span);
        
        packet.Command =
            new UseAbilityCommand(slot, repeat, targetPosition, targetUnitId, targetDirection, targetShardId);
        return packet;
    }
}