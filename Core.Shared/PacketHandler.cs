using System.Diagnostics.CodeAnalysis;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Packets;

namespace Soteo.CampaignServer.PacketHandlers;

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
