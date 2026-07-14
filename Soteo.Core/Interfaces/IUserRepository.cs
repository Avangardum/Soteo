using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Models;

namespace Soteo.Core.Interfaces;

public interface IUserRepository : IDictionary<Guid, User>
{
    void Add(User user);
    void OnConnected(IDictionary<string, object> claims);
    void OnDisconnected(Guid id);
    IReadOnlyDictionary<Guid, UserSnapshot> CreateSnapshot();
    void ReplicateSnapshot(IReadOnlyDictionary<Guid, UserSnapshot> snapshot);
    Task WaitForUsersToConnectAsync(IReadOnlyList<Guid> ids, double timeout);
}
