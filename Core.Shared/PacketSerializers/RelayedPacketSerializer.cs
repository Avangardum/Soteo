using Soteo.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Shared.PacketSerializers;

public abstract class RelayedPacketSerializer<TPacket> : PacketSerializer<TPacket>
    where TPacket : RelayedPacket, new()
{
    protected override void SerializeInternal(TPacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeGuid(packet.PeerId, stream);
    }

    protected override TPacket DeserializeInternal(Stream stream)
    {
        TPacket packet = base.DeserializeInternal(stream);
        packet.PeerId = DeserializeGuid(stream);
        return packet;
    }
}