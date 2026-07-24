using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Models;

// todo summary
public sealed record PlayerCharacterTracker
{
    public required Guid Id { get; init; }
    public required User Player { get; set; }
    public User? Shard { get; set; }
    
    public static PlayerCharacterTracker FromSnapshot
    (
        PlayerCharacterTrackerSnapshot snapshot,
        IUserRepository userRepository
    )
    {
        return new()
        {
            Id = snapshot.Id,
            Player = userRepository[snapshot.PlayerId],
            Shard = snapshot.ShardId?.PassTo(it => userRepository[it]),
        };
    }
    
    public PlayerCharacterTrackerSnapshot ToSnapshot()
    {
        return new PlayerCharacterTrackerSnapshot
        {
            Id = Id,
            PlayerId = Player.Id,
            ShardId = Shard?.Id,
        };
    }
}
