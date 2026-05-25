using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared.PacketSerializers;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Gameplay.PacketSerializers;

public sealed class MovePacketSerializer : PacketSerializer<MovePacket>
{
    protected override void SerializeInternal(MovePacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeVector2(packet.Position, stream);
    }

    protected override MovePacket DeserializeInternal(Stream stream)
    {
        var message = base.DeserializeInternal(stream);
        message.Position = DeserializeVector2(stream);
        return message;
    }
}
