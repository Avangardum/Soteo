namespace Soteo.Core.Serialization.Packets;

public sealed class CampaignServerHandshakePacketSerializer(ISerializationHelper s) :
    PacketSerializer<CampaignServerHandshakePacket>(s)
{
    protected override void SerializeCore(CampaignServerHandshakePacket packet, Stream stream)
    {
        s.SerializeString(packet.Token, stream);
        s.SerializeString(packet.Version, stream);
    }

    protected override CampaignServerHandshakePacket DeserializeCore(Stream stream)
    {
        return new CampaignServerHandshakePacket
        {
            Token = s.DeserializeString(stream),
            Version = s.DeserializeString(stream)
        };
    }
}
