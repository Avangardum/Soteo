using Soteo.Core.Commands;
using Soteo.Core.Packets.Commands;

namespace Soteo.Core.Serialization.Packets.Commands;

public sealed class UnequipPacketSerializer(ISerializationHelper s) : PacketSerializer<UnequipPacket>(s)
{
    protected override void SerializeCore(UnequipPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.UnitId, stream);
        s.SerializeEnum(packet.Command.EquipmentSlot, stream);
        s.SerializeNullableStruct(packet.Command.ToInventorySlot, s.SerializeInt, stream);
    }

    protected override UnequipPacket DeserializeCore(Stream stream)
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
