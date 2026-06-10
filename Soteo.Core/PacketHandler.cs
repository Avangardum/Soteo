using System.Diagnostics.CodeAnalysis;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Exceptions;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Shared;

public abstract class PacketHandler<T> : IPacketHandler where T : Packet
{
    Task IPacketHandler.HandleAsync(Packet packet, Guid senderId) => HandleAsync((T)packet, senderId);
    
    public virtual Task HandleAsync(T packet, Guid senderId)
    {
        Handle(packet, senderId);
        return Task.CompletedTask;
    }
    
    protected virtual void Handle(T packet, Guid senderId) {}
    
    protected void Validate([DoesNotReturnIf(false)] bool condition, string reason)
    {
        if (!condition) throw new BadPacketException(reason);
    }
}
