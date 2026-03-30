using Soteo.Shared.Messages.PlayerShard;

namespace Soteo.Client.MessageHandlers;

public sealed class MoveMessageHandler : MessageHandler<MoveMessage>
{
    protected override void Handle(MoveMessage message, Guid senderId)
    {
        ValidateThisIsServer();
        GD.Print("Move");
    }
}