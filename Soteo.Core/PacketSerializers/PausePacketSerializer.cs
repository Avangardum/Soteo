using Soteo.Core.Interfaces;
using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class PausePacketSerializer(ISerializationHelper s) : PacketSerializer<PausePacket>(s)
{
    protected override void SerializeInternal(PausePacket packet, Stream stream)
    {
        s.SerializeBool(packet.Pause, stream);
    }

    protected override PausePacket DeserializeInternal(Stream stream)
    {
        return new PausePacket { Pause = s.DeserializeBool(stream) };
    }
}
