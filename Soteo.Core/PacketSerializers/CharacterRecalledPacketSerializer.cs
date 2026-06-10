using Soteo.Core.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Shared.PacketSerializers;

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
