using Soteo.Core.CampaignServer.Dto.Snapshots;
using Soteo.Core.CampaignServer.GameState.DataObjects;

namespace Soteo.Core.CampaignServer.Interfaces;

public interface IUserRepository : IDictionary<Guid, User>
{
    void OnConnected(IDictionary<string, object> claims);
    void OnDisconnected(Guid id);
    IReadOnlyDictionary<Guid, UserSnapshot> CreateSnapshot();
}
