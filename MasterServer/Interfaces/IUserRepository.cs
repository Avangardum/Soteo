using System.Security.Claims;
using Soteo.MasterServer.GameState.DataObjects;

namespace Soteo.MasterServer.Interfaces;

public interface IUserRepository : IDictionary<Guid, User>
{
    void OnConnected(ClaimsPrincipal claims);
    void OnDisconnected(Guid id);
}