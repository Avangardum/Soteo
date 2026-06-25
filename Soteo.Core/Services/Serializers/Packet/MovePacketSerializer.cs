using Soteo.Core.Commands;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

public sealed class MovePacketSerializer(ISerializationHelper s) : PacketSerializer<MovePacket>(s)
{
    protected override void SerializeInternal(MovePacket packet, Stream stream)
    {
        s.SerializeGuid(packet.UnitId, stream);
        s.SerializeVector2(packet.Command.Position, stream);
    }

    protected override MovePacket DeserializeInternal(Stream stream)
    {
        return new MovePacket
        {
            UnitId = s.DeserializeGuid(stream),
            Command = new MoveCommand(s.DeserializeVector2(stream)),
        };
    }
}
