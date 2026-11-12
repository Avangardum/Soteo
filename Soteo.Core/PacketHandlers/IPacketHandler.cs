namespace Soteo.Core.PacketHandlers;

public interface IPacketHandler
{
    Task HandleAsync(Packet packet, Guid senderId);
}
