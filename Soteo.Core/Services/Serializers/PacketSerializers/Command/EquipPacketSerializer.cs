using Soteo.Core.Dto.Commands;
using Soteo.Core.Dto.Packets.Command;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers.Command;

public sealed class EquipPacketSerializer(ISerializationHelper s) : PacketSerializer<EquipPacket>(s)
{
    protected override void SerializeInternal(EquipPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.UnitId, stream);
        s.SerializeInt(packet.Command.InventorySlot, stream);
    }

    protected override EquipPacket DeserializeInternal(Stream stream)
    {
        return new EquipPacket
        {
            UnitId = s.DeserializeGuid(stream),
            Command = new EquipCommand(s.DeserializeInt(stream))
        };
    }
}
