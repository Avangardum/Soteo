using Soteo.Core.CampaignServerState.DataObjects;
using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.Interfaces;

public interface IUserRepository : IDictionary<Guid, User>
{
    void OnConnected(IDictionary<string, object> claims);
    void OnDisconnected(Guid id);
    IReadOnlyDictionary<Guid, UserSnapshot> CreateSnapshot();
}
