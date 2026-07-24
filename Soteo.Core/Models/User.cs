using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.Models;

/// <summary>
/// Campaign server object representing a user. User can be any other party connected to the campaign server such as
/// a player client or a shard server.
/// </summary>
public sealed record User
{
    public required Guid Id { get; init; }
    public required bool IsConnected { get; set; }
    public required bool IsPlayer { get; init; }
    public required bool IsShard { get; init; }

    public static User FromSnapshot(UserSnapshot snapshot)
    {
        return new()
        {
            Id = snapshot.Id,
            IsConnected = false,
            IsPlayer = snapshot.IsPlayer,
            IsShard = snapshot.IsShard,
        };
    }
    
    public UserSnapshot ToSnapshot()
    {
        return new UserSnapshot
        {
            Id = Id,
            IsConnected = IsConnected,
            IsPlayer = IsPlayer,
            IsShard = IsShard,
        };
    }
}
