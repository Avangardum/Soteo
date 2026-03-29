using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Messages.Master;

namespace Soteo.MasterServer.MessageHandlers;

public sealed class WebrtcSdpMessageHandler(IMessageSender messageSender, IUserRepository userRepo) :
    MessageHandler<WebrtcSdpMessage>
{
    public override async Task HandleAsync(WebrtcSdpMessage message, User sender)
    {
        if (!userRepo.TryGetValue(message.PeerId, out User? receiver)) return;
        Validate(sender.IsPlayer && receiver.IsShard || sender.IsShard && receiver.IsPlayer,
            "WebRTC signaling can only happen between a player and a shard");
        await messageSender.RelayFromAsync(message, sender.Id);
    }
}