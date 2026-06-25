using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Models;

namespace Soteo.Core.Interfaces;

public interface IUserRepository : IDictionary<Guid, User>
{
    void OnConnected(IDictionary<string, object> claims);
    void OnDisconnected(Guid id);
    IReadOnlyDictionary<Guid, UserSnapshot> CreateSnapshot();
}
