using System.Threading.Tasks;
using Soteo.Shared.Messages.Shared;

namespace Soteo.Client;

public interface IMessageHandler
{
    Task HandleAsync(Message message, Guid senderId);
}