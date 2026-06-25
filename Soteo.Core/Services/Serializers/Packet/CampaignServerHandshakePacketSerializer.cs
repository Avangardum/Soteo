using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

public sealed class CampaignServerHandshakePacketSerializer(ISerializationHelper s) :
    PacketSerializer<CampaignServerHandshakePacket>(s)
{
    protected override void SerializeInternal(CampaignServerHandshakePacket packet, Stream stream)
    {
        s.SerializeString(packet.Token, stream);
        s.SerializeString(packet.Version, stream);
    }

    protected override CampaignServerHandshakePacket DeserializeInternal(Stream stream)
    {
        return new CampaignServerHandshakePacket
        {
            Token = s.DeserializeString(stream),
            Version = s.DeserializeString(stream),
        };
    }
}
