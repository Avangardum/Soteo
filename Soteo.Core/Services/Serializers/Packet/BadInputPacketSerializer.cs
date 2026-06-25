using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

public sealed class BadInputPacketSerializer(ISerializationHelper s) : PacketSerializer<BadInputPacket>(s)
{
    protected override void SerializeInternal(BadInputPacket packet, Stream stream)
    {
        s.SerializeString(packet.Reason, stream);
    }

    protected override BadInputPacket DeserializeInternal(Stream stream)
    {
        return new BadInputPacket { Reason = s.DeserializeString(stream) };
    }
}
