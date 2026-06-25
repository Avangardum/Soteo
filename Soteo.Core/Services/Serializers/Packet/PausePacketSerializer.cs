using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

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
