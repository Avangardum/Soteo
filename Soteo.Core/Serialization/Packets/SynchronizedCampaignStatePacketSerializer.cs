namespace Soteo.Core.Serialization.Packets;

public sealed class SynchronizedCampaignStatePacketSerializer(ISerializationHelper s) :
    PacketSerializer<SynchronizedCampaignStatePacket>(s)
{
    protected override void SerializeCore(SynchronizedCampaignStatePacket packet, Stream stream)
    {
        s.SerializeBool(packet.Value.IsPaused, stream);
    }

    protected override SynchronizedCampaignStatePacket DeserializeCore(Stream stream)
    {
        return new SynchronizedCampaignStatePacket(new SynchronizedCampaignState
        {
            IsPaused = s.DeserializeBool(stream)
        });
    }
}
