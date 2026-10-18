using Soteo.Core.Commands;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class MoveItemStackPacketSerializer(ISerializationHelper s) : PacketSerializer<MoveItemStackPacket>(s)
{
    protected override void SerializeInternal(MoveItemStackPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.UnitId, stream);
        s.SerializeInt(packet.Command.FromIndex, stream);
        s.SerializeInt(packet.Command.ToIndex, stream);
    }

    protected override MoveItemStackPacket DeserializeInternal(Stream stream)
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
