using Soteo.Core.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Shared.PacketSerializers;

public sealed class PausePacketSerializer : PacketSerializer<PausePacket>
{
    protected override void SerializeInternal(PausePacket packet, Stream stream)
    {
        SerializeBool(packet.Pause, stream);
    }

    protected override PausePacket DeserializeInternal(Stream stream)
    {
        return new PausePacket { Pause = DeserializeBool(stream) };
    }
}
