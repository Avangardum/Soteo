using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.CampaignServerState.DataObjects;

public sealed record PlayerCharacterTracker
{
    public required Guid Id { get; init; }
    public User? Player { get; set; } // todo make required
    public Guid? ShardId { get; set; }
    
    public PlayerCharacterSnapshot CreateSnapshot()
    {
        return new PlayerCharacterSnapshot
        {
            Id = Id,
            PlayerId = Player?.Id,
            ShardId = ShardId,
        };
    }
}
