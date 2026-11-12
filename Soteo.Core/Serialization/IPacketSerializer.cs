namespace Soteo.Core.Serialization;

public interface IPacketSerializer
{
    Packets.Packet Deserialize(Span<byte> bytes);
    byte[] Serialize(Packets.Packet packet);
}
