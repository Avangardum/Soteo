using Soteo.Core.CampaignServer.Dto.Snapshots;

namespace Soteo.Core.CampaignServer.GameState.DataObjects;

public sealed record PlayerCharacter
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
