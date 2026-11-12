using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class EmptyPacketSerializer<TPacket>(ISerializationHelper s) :
    PacketSerializer<TPacket>(s) where TPacket : Packet, new()
{
    protected override void SerializeInternal(TPacket packet, Stream stream) { }
    protected override TPacket DeserializeInternal(Stream stream) => new();
}
