using Soteo.Core.Dto.Packets;

namespace Soteo.Core.Interfaces;

public interface IPacketHandler
{
    Task HandleAsync(Packet packet, Guid senderId);
}
