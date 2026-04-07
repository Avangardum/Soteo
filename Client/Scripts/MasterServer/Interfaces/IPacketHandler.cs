using Soteo.MasterServer.GameState.DataObjects;
using Soteo.Shared.Packets.Shared;

namespace Soteo.MasterServer.Interfaces;

public interface IPacketHandler
{
    Task HandleAsync(Packet packet, User user);
}