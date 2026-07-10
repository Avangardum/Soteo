using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Models;

public sealed record PlayerCharacterTracker
{
    public required Guid Id { get; init; }
    public User? Player { get; set; } // todo make required
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
            Player = snapshot.PlayerId?.PassTo(it => userRepository[it]),
            ShardId = snapshot.ShardId,
        };
    }
    
    public PlayerCharacterTrackerSnapshot CreateSnapshot()
    {
        return new PlayerCharacterTrackerSnapshot
        {
            Id = Id,
            PlayerId = Player?.Id,
            ShardId = ShardId,
        };
    }
}
