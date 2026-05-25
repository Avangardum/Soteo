using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.Interfaces;

namespace Soteo.Core.CampaignServer.GameState.Repositories;

public class UserRepository : Dictionary<Guid, User>, IUserRepository
{
    public void OnConnected(IDictionary<string, object> claims)
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
                IsPlayer = claims.ContainsKey("player"),
                IsShard = claims.ContainsKey("shard")
            };
            Add(id, user);
        }
    }
    
    public void OnDisconnected(Guid id)
    {
        if (TryGetValue(id, out User? user))
            user.IsConnected = false;
    }
}
