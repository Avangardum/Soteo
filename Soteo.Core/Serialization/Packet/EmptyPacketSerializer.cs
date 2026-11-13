namespace Soteo.Core.Serialization.Packet;

public sealed class EmptyPacketSerializer<TPacket>(ISerializationHelper s) :
    PacketSerializer<TPacket>(s) where TPacket : Packets.Packet, new()
{
    protected override void SerializeCore(TPacket packet, Stream stream) { }
    protected override TPacket DeserializeCore(Stream stream) => new();
}
