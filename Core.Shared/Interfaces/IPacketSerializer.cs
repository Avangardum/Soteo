using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Shared.Interfaces;

public interface IPacketSerializer
{
    Packet Deserialize(Span<byte> bytes);
    byte[] Serialize(Packet packet);
}
