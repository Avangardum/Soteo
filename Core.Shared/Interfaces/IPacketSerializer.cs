using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Shared.Interfaces;

/// <summary>
/// Serializes and deserializes packets. Thread safe.
/// </summary>
public interface IPacketSerializer
{
    Packet Deserialize(Span<byte> bytes);
    byte[] Serialize(Packet packet);
}