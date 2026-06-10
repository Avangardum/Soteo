using Soteo.Core.Commands;
using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class StopPacketSerializer : PacketSerializer<StopPacket>
{
    protected override void SerializeInternal(StopPacket packet, Stream stream)
    {
        SerializeGuid(packet.UnitId, stream);
    }

    protected override StopPacket DeserializeInternal(Stream stream)
    {
        return new StopPacket
        {
            UnitId = DeserializeGuid(stream),
            Command = new StopCommand(),
        };
    }
}
