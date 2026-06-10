using Soteo.Core.Packets;
using static Soteo.Core.SerializationHelper;

namespace Soteo.Core.PacketSerializers;

public sealed class CharacterRecalledPacketSerializer : PacketSerializer<CharacterRecalledPacket>
{
    protected override void SerializeInternal(CharacterRecalledPacket packet, Stream stream)
    {
        SerializeGuid(packet.CharacterId, stream);
    }

    protected override CharacterRecalledPacket DeserializeInternal(Stream stream)
    {
        return new CharacterRecalledPacket { CharacterId = DeserializeGuid(stream) };
    }
}
