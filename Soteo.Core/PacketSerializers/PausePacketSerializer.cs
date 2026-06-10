using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

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
