using System.Collections.Immutable;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;
using Soteo.Core.Models;
using Soteo.Util;

namespace Soteo.Core.Services.Repositories;

public class UserRepository : Dictionary<Guid, User>, IUserRepository
{
    private TaskCompletionSource _userConnectedTcs = new();
    
    public void Add(User user) => Add(user.Id, user);
    
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
                IsShard = claims.ContainsKey("shard"),
            };
            Add(id, user);
        }
        
        var oldUserConnectedTcs = _userConnectedTcs;
        _userConnectedTcs = new();
        oldUserConnectedTcs.SetResult();
    }
    
    public void OnDisconnected(Guid id)
    {
        if (TryGetValue(id, out User? user))
            user.IsConnected = false;
    }
    
    public IReadOnlyDictionary<Guid, UserSnapshot> CreateSnapshot() =>
        this.ToImmutableDictionary(it => it.Key, it => it.Value.CreateSnapshot());
    
    public void ReplicateSnapshot(IReadOnlyDictionary<Guid, UserSnapshot> snapshot)
    {
        Clear();
        foreach (UserSnapshot userSnapshot in snapshot.Values)
            Add(User.FromSnapshot(userSnapshot));
    }
    
    public async Task WaitForUsersToConnectAsync(params IReadOnlyList<Guid> ids)
    {
    begin:
        //await Task.Delay(5000);
        foreach (Guid id in ids)
        {
            if (!TryGetValue(id, out User user) || !user.IsConnected)
            {
                await _userConnectedTcs.Task;
                goto begin;
            }
        }
    }
}
