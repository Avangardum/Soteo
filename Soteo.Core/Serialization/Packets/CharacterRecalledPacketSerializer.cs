namespace Soteo.Core.Serialization.Packets;

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
