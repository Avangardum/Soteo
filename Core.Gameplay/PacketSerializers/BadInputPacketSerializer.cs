using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

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