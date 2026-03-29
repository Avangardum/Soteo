using Soteo.Shared.Attributes;
using Soteo.Shared.Messages.Master;
using Soteo.Shared.Messages.Shared;

namespace Soteo.MasterServer.Interfaces;

/// <summary>
/// Sends messages to users. Thread safe.
/// </summary>
public interface IMessageSender
{
    Task SendToAsync(Message message, Guid receiverId);
    
    async Task RelayFromAsync(RelayedMessage message, Guid senderId) =>
        await SendToAsync(message with { PeerId = senderId }, message.PeerId);
}