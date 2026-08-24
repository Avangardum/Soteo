using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

// todo remove subclasses, use the base class directly by marking packets with an attribute
public abstract class EmptyPacketSerializer<TPacket>(ISerializationHelper s) :
    PacketSerializer<TPacket>(s) where TPacket : Packet, new()
{
    protected sealed override void SerializeInternal(TPacket packet, Stream stream) { }
    protected sealed override TPacket DeserializeInternal(Stream stream) => new();
}
