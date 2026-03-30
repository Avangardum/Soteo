using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Messages.Shared;

namespace Soteo.Client.MessageHandlers;

public abstract class MessageHandler<T> : IMessageHandler where T : Message
{
    Task IMessageHandler.HandleAsync(Message message, Guid senderId) => HandleAsync((T)message, senderId);
    
    public virtual Task HandleAsync(T message, Guid senderId)
    {
        Handle(message, senderId);
        return Task.CompletedTask;
    }
    
    protected virtual void Handle(T message, Guid senderId) {}
    
    protected void Validate(bool condition, string reason)
    {
        if (!condition) throw new BadMessageException(reason);
    }
    
    protected void ValidateThisIsServer() => Validate(IsServer, "This message can only be handled by a server");
    
    protected void ValidateIsMasterServer(Guid senderId) =>
        Validate(senderId == MasterServerId, "Only the master server can send this");
}