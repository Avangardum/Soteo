using System.Diagnostics.CodeAnalysis;
using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Messages.Shared;

namespace Soteo.MasterServer.MessageHandlers;

public abstract class MessageHandler<T> : IMessageHandler where T : Message
{
    Task IMessageHandler.HandleAsync(Message message, User sender) => HandleAsync((T)message, sender);
    
    public virtual Task HandleAsync(T message, User sender)
    {
        Handle(message, sender);
        return Task.CompletedTask;
    }
    
    protected virtual void Handle(T message, User sender) {}
    
    protected void Validate([DoesNotReturnIf(false)] bool condition, string reason)
    {
        if (!condition) throw new BadMessageException(reason);
    }
}