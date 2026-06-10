using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class BadInputPacketSerializer : PacketSerializer<BadInputPacket>
{
    protected override void SerializeInternal(BadInputPacket packet, Stream stream)
    {
        SerializeString(packet.Reason, stream);
    }

    protected override BadInputPacket DeserializeInternal(Stream stream)
    {
        return new BadInputPacket { Reason = DeserializeString(stream) };
    }
}
