using Soteo.Core.Interfaces;
using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class CharacterRecalledPacketSerializer(ISerializationHelper s) :
    PacketSerializer<CharacterRecalledPacket>(s)
{
    protected override void SerializeInternal(CharacterRecalledPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.CharacterId, stream);
    }

    protected override CharacterRecalledPacket DeserializeInternal(Stream stream)
    {
        return new CharacterRecalledPacket { CharacterId = s.DeserializeGuid(stream) };
    }
}
