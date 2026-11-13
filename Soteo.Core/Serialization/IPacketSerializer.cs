namespace Soteo.Core.Serialization;

public interface IPacketSerializer
{
    Packet Deserialize(Span<byte> bytes);
    byte[] Serialize(Packet packet);
}
