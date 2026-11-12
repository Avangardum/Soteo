namespace Soteo.Core.Serialization.Packet;

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
