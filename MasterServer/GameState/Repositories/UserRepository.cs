using System.Security.Claims;
using Soteo.MasterServer.Extensions;
using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;

namespace Soteo.MasterServer.GameState.Repositories;

public class UserRepository : Dictionary<Guid, User>, IUserRepository
{
    public void OnConnected(ClaimsPrincipal claims)
    {
        Guid id = claims.Id;
        if (TryGetValue(id, out User? user))
        {
            user.IsConnected = true;
        }
        else
        {
            user = new User
            {
                Id = id,
                IsConnected = true,
                IsPlayer = claims.IsPlayer,
                IsShard = claims.IsShard
            };
            Add(id, user);
        }
    }
    
    public void OnDisconnected(Guid id)
    {
        if (TryGetValue(id, out User? user)) user.IsConnected = false;
    }
}