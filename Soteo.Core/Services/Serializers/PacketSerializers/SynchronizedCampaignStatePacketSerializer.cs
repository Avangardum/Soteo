using Soteo.Core.Dto;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class SynchronizedCampaignStatePacketSerializer(ISerializationHelper s) :
    PacketSerializer<SynchronizedCampaignStatePacket>(s)
{
    protected override void SerializeInternal(SynchronizedCampaignStatePacket packet, Stream stream)
    {
        s.SerializeBool(packet.Value.IsPaused, stream);
    }

    protected override SynchronizedCampaignStatePacket DeserializeInternal(Stream stream)
    {
        return new SynchronizedCampaignStatePacket(new SynchronizedCampaignState
        {
            IsPaused = s.DeserializeBool(stream),
        });
    }
}
