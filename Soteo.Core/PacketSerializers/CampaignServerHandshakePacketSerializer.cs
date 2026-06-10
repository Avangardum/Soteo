using Soteo.Core.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Shared.PacketSerializers;

public sealed class CampaignServerHandshakePacketSerializer : PacketSerializer<CampaignServerHandshakePacket>
{
    protected override void SerializeInternal(CampaignServerHandshakePacket packet, Stream stream)
    {
        SerializeString(packet.Token, stream);
        SerializeString(packet.Version, stream);
    }

    protected override CampaignServerHandshakePacket DeserializeInternal(Stream stream)
    {
        return new CampaignServerHandshakePacket
        {
            Token = DeserializeString(stream),
            Version = DeserializeString(stream),
        };
    }
}
