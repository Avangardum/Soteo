using Soteo.Core.Commands;
using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class MovePacketSerializer : PacketSerializer<MovePacket>
{
    protected override void SerializeInternal(MovePacket packet, Stream stream)
    {
        SerializeGuid(packet.UnitId, stream);
        SerializeVector2(packet.Command.Position, stream);
    }

    protected override MovePacket DeserializeInternal(Stream stream)
    {
        return new MovePacket
        {
            UnitId = DeserializeGuid(stream),
            Command = new MoveCommand(DeserializeVector2(stream)),
        };
    }
}
