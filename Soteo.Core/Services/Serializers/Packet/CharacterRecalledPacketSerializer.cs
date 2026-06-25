using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

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
