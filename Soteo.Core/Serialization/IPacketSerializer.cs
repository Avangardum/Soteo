using Soteo.Core.Dto.Packets;

namespace Soteo.Core.Interfaces;

public interface IPacketSerializer
{
    Packet Deserialize(Span<byte> bytes);
    byte[] Serialize(Packet packet);
}
