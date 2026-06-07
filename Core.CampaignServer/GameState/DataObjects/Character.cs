namespace Soteo.Core.CampaignServer.GameState.DataObjects;

public sealed record Character
{
    public required Guid Id { get; set; }
    public Guid? ShardId { get; set; }
}
