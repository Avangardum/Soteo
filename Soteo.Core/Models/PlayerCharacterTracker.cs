using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Models;

// todo summary
public sealed record PlayerCharacterTracker
{
    public required Guid Id { get; init; }
    public required User Player { get; set; }
    public Guid? ShardId { get; set; } // todo make Shard
    
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
            ShardId = snapshot.ShardId,
        };
    }
    
    public PlayerCharacterTrackerSnapshot ToSnapshot()
    {
        return new PlayerCharacterTrackerSnapshot
        {
            Id = Id,
            PlayerId = Player.Id,
            ShardId = ShardId,
        };
    }
}
