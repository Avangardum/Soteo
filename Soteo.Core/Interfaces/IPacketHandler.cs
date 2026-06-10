using Soteo.Core.Packets;

namespace Soteo.Core.Interfaces;

public interface IPacketHandler
{
    Task HandleAsync(Packet packet, Guid senderId);
}
