using Soteo.Core.Commands;
using Soteo.Core.Enums;
using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class UseAbilityPacketSerializer : PacketSerializer<UseAbilityPacket>
{
    protected override void SerializeInternal(UseAbilityPacket packet, Stream stream)
    {
        SerializeGuid(packet.UnitId, stream);
        
        SerializeEnum(packet.Command.Slot, stream);
        SerializeBool(packet.Command.Repeat, stream);
        
        SerializeNullableStruct(packet.Command.TargetPosition, SerializeVector2, stream);
        SerializeNullableStruct(packet.Command.TargetUnitId, SerializeGuid, stream);
        SerializeNullableStruct(packet.Command.TargetDirection, SerializeVector2, stream);
        SerializeNullableStruct(packet.Command.TargetShardId, SerializeGuid, stream);
    }

    protected override UseAbilityPacket DeserializeInternal(Stream stream)
    {
        return new UseAbilityPacket
        {
            UnitId = DeserializeGuid(stream),
            Command = new UseAbilityCommand
            (
                Slot: DeserializeEnum<AbilitySlot>(stream),
                Repeat: DeserializeBool(stream),
                TargetPosition: DeserializeNullableStruct(DeserializeVector2, stream),
                TargetUnitId: DeserializeNullableStruct(DeserializeGuid, stream),
                TargetDirection: DeserializeNullableStruct(DeserializeVector2, stream),
                TargetShardId: DeserializeNullableStruct(DeserializeGuid, stream)
            ),
        };
    }
}
