using Soteo.Core.Commands;

namespace Soteo.Core.Serialization.Packets.Commands;

public sealed class EquipPacketSerializer(ISerializationHelper s) : PacketSerializer<EquipPacket>(s)
{
    protected override void SerializeCore(EquipPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.UnitId, stream);
        s.SerializeInt(packet.Command.InventorySlot, stream);
    }

    protected override EquipPacket DeserializeCore(Stream stream)
    {
        return new EquipPacket
        {
            UnitId = s.DeserializeGuid(stream),
            Command = new EquipCommand(s.DeserializeInt(stream))
        };
    }
}
