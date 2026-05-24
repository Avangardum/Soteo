using Soteo.Core.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Shared.PacketSerializers;

public sealed class CharacterRecalledPacketSerializer : PacketSerializer<CharacterRecalledPacket>
{
    protected override void SerializeInternal(CharacterRecalledPacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeGuid(packet.CharacterId, stream);
    }

    protected override CharacterRecalledPacket DeserializeInternal(Stream stream)
    {
        var packet = base.DeserializeInternal(stream);
        packet.CharacterId = DeserializeGuid(stream);
        return packet;
    }
}