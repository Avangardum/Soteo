using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared.PacketSerializers;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Gameplay.PacketSerializers;

public sealed class MovePacketSerializer : PacketSerializer<MovePacket>
{
    protected override void SerializeInternal(MovePacket packet, Stream stream)
    {
        SerializeVector2(packet.Position, stream);
    }

    protected override MovePacket DeserializeInternal(Stream stream)
    {
        return new MovePacket { Position = DeserializeVector2(stream) };
    }
}
