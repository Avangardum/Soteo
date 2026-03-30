using System.Diagnostics.CodeAnalysis;
using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Packets.Shared;

namespace Soteo.MasterServer.PacketHandlers;

public abstract class PacketHandler<T> : IPacketHandler where T : Packet
{
    Task IPacketHandler.HandleAsync(Packet packet, User sender) => HandleAsync((T)packet, sender);
    
    public virtual Task HandleAsync(T packet, User sender)
    {
        Handle(packet, sender);
        return Task.CompletedTask;
    }
    
    protected virtual void Handle(T packet, User sender) {}
    
    protected void Validate([DoesNotReturnIf(false)] bool condition, string reason)
    {
        if (!condition) throw new BadPacketException(reason);
    }
}