using System.Collections.Immutable;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;
using Soteo.Core.Models;
using Soteo.Util;

namespace Soteo.Core.Services.Repositories;

public class UserRepository(IShardServerAllowlist shardServerAllowlist) : Dictionary<Guid, User>, IUserRepository
{
    private TaskCompletionSource _userConnectedTcs = new();
    
    public void Add(User user) => Add(user.Id, user);
    
    public void OnConnected(IDictionary<string, object> claims)
    {
        Guid id = Guid.Parse((string)claims["sub"]);
        bool isShard = claims.ContainsKey("shard");
        
        if (isShard && shardServerAllowlist.IsEnabled && !shardServerAllowlist.AllowedShardIds.Contains(id))
            throw new Exception($"Unexpected connection from shard server {id}");
        
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
                IsShard = isShard,
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
    
    public async Task WaitForUsersToConnectAsync(IReadOnlyList<Guid> ids, double timeout)
    {
        Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeout));
        
    retry:
        foreach (Guid id in ids)
        {
            if (!TryGetValue(id, out User user) || !user.IsConnected)
            {
                Task completedTask = await Task.WhenAny(_userConnectedTcs.Task, timeoutTask);
                if (completedTask == timeoutTask)
                    throw new TimeoutException($"The following users did not connect: {ids.JoinToString(", ")}");
                goto retry;
            }
        }
    }
}
