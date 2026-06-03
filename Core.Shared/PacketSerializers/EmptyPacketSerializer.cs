using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Shared.PacketSerializers;

public abstract class EmptyPacketSerializer<TPacket> : PacketSerializer<TPacket> where TPacket : Packet, new()
{
    protected sealed override void SerializeInternal(TPacket packet, Stream stream) =>
        base.SerializeInternal(packet, stream);

    protected sealed override TPacket DeserializeInternal(Stream stream) => new();
}
