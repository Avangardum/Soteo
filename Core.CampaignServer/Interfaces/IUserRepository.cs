using Soteo.CampaignServer.GameState.DataObjects;

namespace Soteo.CampaignServer.Interfaces;

public interface IUserRepository : IDictionary<Guid, User>
{
    void OnConnected(IDictionary<string, object> claims);
    void OnDisconnected(Guid id);
}