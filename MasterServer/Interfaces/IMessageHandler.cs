using Soteo.MasterServer.GameState.DataObjects;
using Soteo.Shared.Messages.Shared;

namespace Soteo.MasterServer.Interfaces;

public interface IMessageHandler
{
    Task HandleAsync(Message message, User user);
}