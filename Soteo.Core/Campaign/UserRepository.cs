
namespace Soteo.Core.Campaign;

/// <inheritdoc cref="IUserRepository" />
public class UserRepository
(
    IShardServerAllowlist shardServerAllowlist,
    TimeProvider timeProvider
) : Dictionary<Guid, User>, IUserRepository
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
                IsShard = isShard
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

    public IReadOnlyDictionary<Guid, UserSnapshot> ToSnapshot() =>
        this.ToDictionary(it => it.Key, it => it.Value.ToSnapshot()).AsReadOnly();

    public void ReplicateSnapshot(IReadOnlyDictionary<Guid, UserSnapshot> snapshot)
    {
        Clear();
        foreach (UserSnapshot userSnapshot in snapshot.Values)
            Add(User.FromSnapshot(userSnapshot));
    }

    public async Task WaitForUsersToConnectAsync(IReadOnlyList<Guid> ids, double timeout)
    {
        Task timeoutTask = timeProvider.Delay(TimeSpan.FromSeconds(timeout));

    retry:
        foreach (Guid id in ids)
        {
            if (!TryGetValue(id, out User? user) || !user.IsConnected)
            {
                Task completedTask = await Task.WhenAny(_userConnectedTcs.Task, timeoutTask);
                if (completedTask == timeoutTask)
                    throw new TimeoutException($"The following users did not connect: {ids.JoinToString(", ")}");
                goto retry;
            }
        }
    }
}
