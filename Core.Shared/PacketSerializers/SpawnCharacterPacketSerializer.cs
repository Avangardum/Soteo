using Soteo.Core.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Shared.PacketSerializers;

public sealed class SpawnCharacterPacketSerializer : PacketSerializer<SpawnCharacterPacket>
{
    protected override void SerializeInternal(SpawnCharacterPacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
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
