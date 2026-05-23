using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface IPacketHandler
{
    Task HandleAsync(Packet packet, Guid senderId);
}