using Soteo.Core.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Shared.PacketSerializers;

public sealed class BadInputPacketSerializer : PacketSerializer<BadInputPacket>
{
    protected override void SerializeInternal(BadInputPacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeString(packet.Reason, stream);
    }

    protected override BadInputPacket DeserializeInternal(Stream stream)
    {
        var packet = base.DeserializeInternal(stream);
        packet.Reason = DeserializeString(stream);
        return packet;
    }
}