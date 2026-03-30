using Soteo.Shared.Messages.Shared;

namespace Soteo.Client;

public interface IMasterServerCommunicator : IMessageSender
{
    event Action ConnectionEstablished;
    void ConnectAsPlayer(string email, string password);
    void ConnectAsShardServer();
    void SendMessage(Message message);
}