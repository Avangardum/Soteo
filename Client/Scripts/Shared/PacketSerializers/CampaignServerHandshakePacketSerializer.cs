using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class CampaignServerHandshakePacketSerializer : PacketSerializer<CampaignServerHandshakePacket>
{
    protected override void SerializeInternal(CampaignServerHandshakePacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeString(packet.Token, stream);
        SerializeString(packet.Version, stream);
    }

    protected override CampaignServerHandshakePacket DeserializeInternal(Stream stream)
    {
        var packet = base.DeserializeInternal(stream);
        packet.Token = DeserializeString(stream);
        packet.Version = DeserializeString(stream);
        return packet;
    }
}