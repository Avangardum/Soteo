namespace Soteo.Core.CampaignServer.GameState.DataObjects;

public sealed record PlayerCharacter
{
    public required Guid Id { get; init; }
    public User? Player { get; set; } // todo make required
    public Guid? ShardId { get; set; }
}
