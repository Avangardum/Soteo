using Soteo.Core.Commands;
using Soteo.Core.Packets.Commands;

namespace Soteo.Core.Serialization.Packets.Commands;

public sealed class MoveItemStackPacketSerializer(ISerializationHelper s) : PacketSerializer<MoveItemStackPacket>(s)
{
    protected override void SerializeCore(MoveItemStackPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.UnitId, stream);
        s.SerializeInt(packet.Command.FromIndex, stream);
        s.SerializeInt(packet.Command.ToIndex, stream);
    }

    protected override MoveItemStackPacket DeserializeCore(Stream stream)
    {
        return new MoveItemStackPacket
        {
            UnitId = s.DeserializeGuid(stream),
            Command = new MoveItemStackCommand
            {
                FromIndex = s.DeserializeInt(stream),
                ToIndex = s.DeserializeInt(stream)
            }
        };
    }
}
