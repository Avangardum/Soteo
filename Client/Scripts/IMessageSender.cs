using Soteo.Shared.Messages.Shared;

namespace Soteo.Client;

public interface IMessageSender
{
    void SendTo(Message message, Guid receiverId);
}