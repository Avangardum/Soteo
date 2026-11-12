namespace Soteo.Core.Synchronization.Snapshots;

public sealed record CampaignSnapshot
{
    public required CampaignServerSnapshot CampaignServer { get; init; }
    public required IReadOnlyDictionary<Guid, ShardSnapshot> Shards { get; init; }
}
