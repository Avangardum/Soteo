using Soteo.Core.Commands;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

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
