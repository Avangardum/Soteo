using Soteo.Core.Dto.Commands;
using Soteo.Core.Dto.Packets.Command;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers.Command;

public sealed class UnequipPacketSerializer(ISerializationHelper s) : PacketSerializer<UnequipPacket>(s)
{
    protected override void SerializeInternal(UnequipPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.UnitId, stream);
        s.SerializeEnum(packet.Command.EquipmentSlot, stream);
        s.SerializeNullableStruct(packet.Command.ToInventorySlot, s.SerializeInt, stream);
    }

    protected override UnequipPacket DeserializeInternal(Stream stream)
    {
        return new UnequipPacket
        {
            UnitId = s.DeserializeGuid(stream),
            Command = new UnequipCommand
            (
                EquipmentSlot: s.DeserializeEnum<EquipmentSlot>(stream),
                ToInventorySlot: s.DeserializeNullableStruct(s.DeserializeInt, stream)
            )
        };
    }
}
