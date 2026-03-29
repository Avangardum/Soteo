using Soteo.Shared.Messages.Shared;

namespace Soteo.Client;

public sealed class UniversalMessageSender(IMessageSender masterSender, IMessageSender fallbackSender) :
    IMessageSender
{
    public void SendTo(Message message, Guid receiverId)
    {
        IMessageSender subSender = receiverId == MasterServerId ? masterSender : fallbackSender;
        subSender.SendTo(message, receiverId);
    }
}