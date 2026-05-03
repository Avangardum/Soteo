using Soteo.CampaignServer.GameState.DataObjects;
using Soteo.Shared.Packets;

namespace Soteo.CampaignServer.Interfaces;

public interface IPacketHandler
{
    Task HandleAsync(Packet packet, User sender);
}