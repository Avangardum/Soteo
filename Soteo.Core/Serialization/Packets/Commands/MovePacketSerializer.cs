using Soteo.Core.Commands;
using Soteo.Core.Packets.Commands;

namespace Soteo.Core.Serialization.Packets.Commands;

public sealed class MovePacketSerializer(ISerializationHelper s) : PacketSerializer<MovePacket>(s)
{
    protected override void SerializeCore(MovePacket packet, Stream stream)
    {
        s.SerializeGuid(packet.UnitId, stream);
        s.SerializeVector2(packet.Command.Position, stream);
    }

    protected override MovePacket DeserializeCore(Stream stream)
    {
        return new MovePacket
        {
            UnitId = s.DeserializeGuid(stream),
            Command = new MoveCommand(s.DeserializeVector2(stream)),
        };
    }
}
