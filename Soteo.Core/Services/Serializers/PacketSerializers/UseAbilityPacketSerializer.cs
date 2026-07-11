using Soteo.Core.Commands;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class UseAbilityPacketSerializer(ISerializationHelper s) : PacketSerializer<UseAbilityPacket>(s)
{
    protected override void SerializeInternal(UseAbilityPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.UnitId, stream);
        
        s.SerializeEnum(packet.Command.Slot, stream);
        s.SerializeBool(packet.Command.Repeat, stream);
        
        s.SerializeNullableStruct(packet.Command.TargetPosition, s.SerializeVector2, stream);
        s.SerializeNullableStruct(packet.Command.TargetUnitId, s.SerializeGuid, stream);
        s.SerializeNullableStruct(packet.Command.TargetDirection, s.SerializeVector2, stream);
        s.SerializeNullableStruct(packet.Command.TargetShardId, s.SerializeGuid, stream);
    }

    protected override UseAbilityPacket DeserializeInternal(Stream stream)
    {
        return new UseAbilityPacket
        {
            UnitId = s.DeserializeGuid(stream),
            Command = new UseAbilityCommand
            (
                Slot: s.DeserializeEnum<AbilitySlot>(stream),
                Repeat: s.DeserializeBool(stream),
                TargetPosition: s.DeserializeNullableStruct(s.DeserializeVector2, stream),
                TargetUnitId: s.DeserializeNullableStruct(s.DeserializeGuid, stream),
                TargetDirection: s.DeserializeNullableStruct(s.DeserializeVector2, stream),
                TargetShardId: s.DeserializeNullableStruct(s.DeserializeGuid, stream)
            ),
        };
    }
}
