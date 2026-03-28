using Soteo.Shared.Messages.Shared;

namespace Soteo.Shared.Interfaces;

public interface IMessageSerializer
{
    Message Deserialize(Span<byte> bytes);
    byte[] Serialize(Message message);
}