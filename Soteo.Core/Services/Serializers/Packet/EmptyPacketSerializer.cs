using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

public abstract class EmptyPacketSerializer<TPacket>(ISerializationHelper s) :
    PacketSerializer<TPacket>(s) where TPacket : Dto.Packets.Packet, new()
{
    protected sealed override void SerializeInternal(TPacket packet, Stream stream) { }
    protected sealed override TPacket DeserializeInternal(Stream stream) => new();
}
