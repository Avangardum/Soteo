using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Messages.Master;

namespace Soteo.MasterServer.MessageHandlers;

public sealed class SpawnCharacterMessageHandler
(
    IUserRepository userRepo,
    ICharacterRepository charRepo,
    IMessageSender messageSender
) : MessageHandler<SpawnCharacterMessage>
{
    public override async Task HandleAsync(SpawnCharacterMessage message, User sender)
    {
        userRepo.TryGetValue(message.PeerId, out User? receiver);
        Validate(sender.IsPlayer && receiver is { IsShard: true });
        if (!charRepo.TryGetValue(sender.Id, out Character? character))
        {
            character = new Character { Id = sender.Id };
            charRepo.Add(character);
        }
        Validate(character.ShardId == Guid.Empty);
        character.ShardId = message.PeerId;
        await messageSender.RelayFromAsync(message, sender.Id);
    }
}