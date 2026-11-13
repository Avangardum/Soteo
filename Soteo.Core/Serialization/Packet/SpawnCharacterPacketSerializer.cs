namespace Soteo.Core.Serialization.Packet;

public sealed class SpawnCharacterPacketSerializer(ISerializationHelper s) : PacketSerializer<SpawnCharacterPacket>(s)
{
    protected override void SerializeCore(SpawnCharacterPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.PeerId, stream);
        s.SerializeGuid(packet.CharacterId, stream);
    }

    protected override SpawnCharacterPacket DeserializeCore(Stream stream)
    {
        return new SpawnCharacterPacket
        {
            PeerId = s.DeserializeGuid(stream),
            CharacterId = s.DeserializeGuid(stream)
        };
    }
}
