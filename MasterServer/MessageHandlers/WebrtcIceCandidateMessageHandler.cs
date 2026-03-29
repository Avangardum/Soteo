using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Messages.Master;

namespace Soteo.MasterServer.MessageHandlers;

public sealed class WebrtcIceCandidateMessageHandler(IMessageSender messageSender, IUserRepository userRepo) :
    MessageHandler<WebrtcIceCandidateMessage>
{
    public override async Task HandleAsync(WebrtcIceCandidateMessage message, User sender)
    {
        if (!userRepo.TryGetValue(message.PeerId, out User? receiver)) return;
        Validate(sender.IsPlayer && receiver.IsShard || sender.IsShard && receiver.IsPlayer, "WebRTC signaling can only happen between a player and a shard");
        await messageSender.RelayFromAsync(message, sender.Id);
    }
}