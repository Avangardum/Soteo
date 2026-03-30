using Soteo.Shared.Messages.Shared;

namespace Soteo.Client;

public sealed class UniversalMessageSender
(
    IMasterServerCommunicator masterSender,
    IMessageSender clientShardServerSender
) : IMessageSender
{
    public void SendReliable(Message message, Guid receiverId)
    {
        if (receiverId == MasterServerId) masterSender.SendMessage(message);
        else clientShardServerSender.SendReliable(message, receiverId);
    }
}