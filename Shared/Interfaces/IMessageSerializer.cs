using Soteo.Shared.Attributes;
using Soteo.Shared.Messages.Shared;

namespace Soteo.Shared.Interfaces;

/// <summary>
/// Serializes and deserializes messages. Thread safe.
/// </summary>
public interface IMessageSerializer
{
    Message Deserialize(Span<byte> bytes);
    byte[] Serialize(Message message);
}