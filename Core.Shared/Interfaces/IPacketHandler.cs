using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Shared.Interfaces;

public interface IPacketHandler
{
    Task HandleAsync(Packet packet, Guid senderId);
}