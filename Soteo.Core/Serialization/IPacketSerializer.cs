namespace Soteo.Core.Serialization;

public interface IPacketSerializer
{
    Packet Deserialize(byte[] bytes);
    byte[] Serialize(Packet packet);
}
