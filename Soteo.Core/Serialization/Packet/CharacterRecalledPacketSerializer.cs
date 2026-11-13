namespace Soteo.Core.Serialization.Packet;

public sealed class CharacterRecalledPacketSerializer(ISerializationHelper s) :
    PacketSerializer<CharacterRecalledPacket>(s)
{
    protected override void SerializeCore(CharacterRecalledPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.CharacterId, stream);
    }

    protected override CharacterRecalledPacket DeserializeCore(Stream stream)
    {
        return new CharacterRecalledPacket { CharacterId = s.DeserializeGuid(stream) };
    }
}
