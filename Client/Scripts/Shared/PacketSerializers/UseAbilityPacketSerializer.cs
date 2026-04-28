using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Enums;
using Soteo.Shared.Enums;
using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class UseAbilityPacketSerializer : PacketSerializer<UseAbilityPacket>
{
    protected override void SerializeInternal(UseAbilityPacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeEnum(packet.Command.Slot, stream);
        SerializeBool(packet.Command.Repeat, stream);
        
        SerializeNullableStruct(packet.Command.TargetPosition, SerializeVector2, stream);
        SerializeNullableStruct(packet.Command.TargetUnitId, SerializeGuid, stream);
        SerializeNullableStruct(packet.Command.TargetDirection, SerializeVector2, stream);
        SerializeNullableStruct(packet.Command.TargetShardId, SerializeGuid, stream);
    }

    protected override UseAbilityPacket DeserializeInternal(Stream stream)
    {
        var packet = base.DeserializeInternal(stream);
        var slot = DeserializeEnum<AbilitySlot>(stream);
        var repeat = DeserializeBool(stream);
        
        Vector2? targetPosition = DeserializeNullableStruct(DeserializeVector2, stream);
        Guid? targetUnitId = DeserializeNullableStruct(DeserializeGuid, stream);
        Vector2? targetDirection = DeserializeNullableStruct(DeserializeVector2, stream);
        Guid? targetShardId = DeserializeNullableStruct(DeserializeGuid, stream);
        
        packet.Command =
            new UseAbilityCommand(slot, repeat, targetPosition, targetUnitId, targetDirection, targetShardId);
        return packet;
    }
}