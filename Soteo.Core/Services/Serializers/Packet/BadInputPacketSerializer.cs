using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketSerializers;

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
