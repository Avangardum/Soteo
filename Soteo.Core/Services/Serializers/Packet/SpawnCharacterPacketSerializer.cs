using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

public sealed class SpawnCharacterPacketSerializer(ISerializationHelper s) : PacketSerializer<SpawnCharacterPacket>(s)
{
    protected override void SerializeInternal(SpawnCharacterPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.PeerId, stream);
        s.SerializeGuid(packet.CharacterId, stream);
    }

    protected override SpawnCharacterPacket DeserializeInternal(Stream stream)
    {
        return new SpawnCharacterPacket
        {
            PeerId = s.DeserializeGuid(stream),
            CharacterId = s.DeserializeGuid(stream),
        };
    }
}
