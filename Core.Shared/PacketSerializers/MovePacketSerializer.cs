using Soteo.Core.Gameplay.Commands;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared.PacketSerializers;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Gameplay.PacketSerializers;

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
