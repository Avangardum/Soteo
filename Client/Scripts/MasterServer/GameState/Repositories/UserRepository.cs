using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;

namespace Soteo.MasterServer.GameState.Repositories;

public class UserRepository : Dictionary<Guid, User>, IUserRepository
{
    public void OnConnected(Dictionary<string, object> claims)
    {
        Guid id = Guid.Parse((string)claims["sub"]);
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
                IsPlayer = (bool)claims["IsPlayer"],
                IsShard = (bool)claims["IsShard"]
            };
            Add(id, user);
        }
    }
    
    public void OnDisconnected(Guid id)
    {
        if (TryGetValue(id, out User? user)) user.IsConnected = false;
    }
}