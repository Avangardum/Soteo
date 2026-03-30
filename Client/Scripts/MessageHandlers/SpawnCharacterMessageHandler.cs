using Soteo.Shared.Messages.Master;

namespace Soteo.Client.MessageHandlers;

public class SpawnCharacterMessageHandler(ICharacterSpawner spawner) : MessageHandler<SpawnCharacterMessage>
{
    protected override void Handle(SpawnCharacterMessage message, Guid senderId)
    {
        ValidateThisIsServer();
        ValidateIsMasterServer(senderId);
        spawner.SpawnPlayerCharacter(message.PeerId);
    }
}