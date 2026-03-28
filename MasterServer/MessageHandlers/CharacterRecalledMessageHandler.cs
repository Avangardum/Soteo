using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Messages.Master;

namespace Soteo.MasterServer.MessageHandlers;

public sealed class CharacterRecalledMessageHandler(ICharacterRepository characterRepository) :
    MessageHandler<CharacterRecalledMessage>
{
    protected override void Handle(CharacterRecalledMessage message, User sender)
    {
        characterRepository.TryGetValue(message.CharacterId, out Character? character);
        Validate(sender.IsShard && character?.ShardId == sender.Id);
        character.ShardId = Guid.Empty;
    }
}