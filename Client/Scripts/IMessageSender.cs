using Soteo.Shared.Messages.Shared;

namespace Soteo.Client;

public interface IMessageSender
{
    void SendReliable(Message message, Guid receiverId);
}