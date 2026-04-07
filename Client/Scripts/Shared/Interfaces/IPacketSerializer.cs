using Soteo.Shared.Attributes;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.Interfaces;

/// <summary>
/// Serializes and deserializes packets. Thread safe.
/// </summary>
public interface IPacketSerializer
{
    Packet Deserialize(Span<byte> bytes);
    byte[] Serialize(Packet packet);
}