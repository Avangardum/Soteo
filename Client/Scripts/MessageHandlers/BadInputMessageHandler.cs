using Soteo.Shared.Exceptions;
using Soteo.Shared.Messages.Master;

namespace Soteo.Client.MessageHandlers;

public class BadInputMessageHandler : MessageHandler<BadInputMessage>
{
    protected override void Handle(BadInputMessage message, Guid senderId)
    {
        Validate(!IsServer || senderId == MasterServerId, "Clients can't report bad input");
        throw new BadMessageException(message.Reason);
    }
}