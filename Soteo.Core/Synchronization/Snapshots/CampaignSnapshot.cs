namespace Soteo.Core.Dto.Snapshots;

public sealed record CampaignSnapshot
{
    public required CampaignServerSnapshot CampaignServer { get; init; }
    public required IReadOnlyDictionary<Guid, ShardSnapshot> Shards { get; init; }
}
