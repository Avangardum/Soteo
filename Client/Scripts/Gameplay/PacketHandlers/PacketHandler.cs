using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public abstract class PacketHandler<T> : IPacketHandler where T : Packet
{
    Task IPacketHandler.HandleAsync(Packet packet, Guid senderId) => HandleAsync((T)packet, senderId);
    
    public virtual Task HandleAsync(T packet, Guid senderId)
    {
        Handle(packet, senderId);
        return Task.CompletedTask;
    }
    
    protected virtual void Handle(T packet, Guid senderId) {}
}