namespace Soteo.Core.Serialization.Packets;

public sealed class EmptyPacketSerializer<TPacket>(SerializationHelper s) :
    PacketSerializer<TPacket>(s) where TPacket : Packet, new()
{
    protected override void SerializeCore(TPacket packet, Stream stream) { }
    protected override TPacket DeserializeCore(Stream stream) => new();
}
