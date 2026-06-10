using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class SpawnCharacterPacketSerializer : PacketSerializer<SpawnCharacterPacket>
{
    protected override void SerializeInternal(SpawnCharacterPacket packet, Stream stream)
    {
        SerializeGuid(packet.PeerId, stream);
        SerializeGuid(packet.CharacterId, stream);
    }

    protected override SpawnCharacterPacket DeserializeInternal(Stream stream)
    {
        return new SpawnCharacterPacket
        {
            PeerId = DeserializeGuid(stream),
            CharacterId = DeserializeGuid(stream),
        };
    }
}
