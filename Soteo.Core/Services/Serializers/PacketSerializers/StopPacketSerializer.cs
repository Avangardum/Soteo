using Soteo.Core.Commands;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class StopPacketSerializer(ISerializationHelper s) : PacketSerializer<StopPacket>(s)
{
    protected override void SerializeInternal(StopPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.UnitId, stream);
    }

    protected override StopPacket DeserializeInternal(Stream stream)
    {
        return new StopPacket
        {
            UnitId = s.DeserializeGuid(stream),
            Command = new StopCommand(),
        };
    }
}
