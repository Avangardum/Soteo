using Soteo.MasterServer.GameState.DataObjects;
using Soteo.Shared.Packets;

namespace Soteo.MasterServer.Interfaces;

public interface IPacketHandler
{
    Task HandleAsync(Packet packet, User sender);
}