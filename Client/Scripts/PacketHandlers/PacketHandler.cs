using System.Threading.Tasks;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Client.PacketHandlers;

public abstract class PacketHandler<T> : IPacketHandler where T : Packet
{
    Task IPacketHandler.HandleAsync(Packet packet, Guid senderId) => HandleAsync((T)packet, senderId);
    
    public virtual Task HandleAsync(T packet, Guid senderId)
    {
        Handle(packet, senderId);
        return Task.CompletedTask;
    }
    
    protected virtual void Handle(T packet, Guid senderId) {}
    
    protected void Validate(bool condition, string reason)
    {
        if (!condition) throw new BadPacketException(reason);
    }
    
    protected void ValidateThisIsServer() => Validate(IsServer, "This packet can only be handled by a server");
    
    protected void ValidateIsMasterServer(Guid senderId) =>
        Validate(senderId == MasterServerId, "Only the master server can send this");
}