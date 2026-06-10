using Soteo.Core.Gameplay.Commands;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared.PacketSerializers;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Gameplay.PacketSerializers;

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
