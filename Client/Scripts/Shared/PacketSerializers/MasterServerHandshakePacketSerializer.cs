using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class MasterServerHandshakePacketSerializer : PacketSerializer<MasterServerHandshakePacket>
{
    protected override void SerializeInternal(MasterServerHandshakePacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeString(packet.Token, stream);
        SerializeString(packet.Version, stream);
    }

    protected override MasterServerHandshakePacket DeserializeInternal(Stream stream)
    {
        var packet = base.DeserializeInternal(stream);
        packet.Token = DeserializeString(stream);
        packet.Version = DeserializeString(stream);
        return packet;
    }
}