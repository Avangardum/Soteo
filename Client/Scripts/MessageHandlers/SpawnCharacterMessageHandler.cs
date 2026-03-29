using Soteo.Shared.Messages.Master;

namespace Soteo.Client.MessageHandlers;

public class SpawnCharacterMessageHandler : MessageHandler<SpawnCharacterMessage>
{
    protected override void Handle(SpawnCharacterMessage message, Guid senderId)
    {
        ValidateThisIsServer();
        ValidateIsMasterServer(senderId);
        GD.Print("Spawn"); // todo spawn character
    }
}