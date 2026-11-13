using Soteo.Core.Commands;

namespace Soteo.Core.Serialization.Packets.Commands;

public sealed class StopPacketSerializer(ISerializationHelper s) : PacketSerializer<StopPacket>(s)
{
    protected override void SerializeCore(StopPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.UnitId, stream);
    }

    protected override StopPacket DeserializeCore(Stream stream)
    {
        return new StopPacket
        {
            UnitId = s.DeserializeGuid(stream),
            Command = new StopCommand()
        };
    }
}
