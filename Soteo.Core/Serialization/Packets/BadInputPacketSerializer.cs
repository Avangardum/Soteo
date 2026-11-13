namespace Soteo.Core.Serialization.Packets;

public sealed class BadInputPacketSerializer(ISerializationHelper s) : PacketSerializer<BadInputPacket>(s)
{
    protected override void SerializeCore(BadInputPacket packet, Stream stream)
    {
        s.SerializeString(packet.Reason, stream);
    }

    protected override BadInputPacket DeserializeCore(Stream stream)
    {
        return new BadInputPacket { Reason = s.DeserializeString(stream) };
    }
}
