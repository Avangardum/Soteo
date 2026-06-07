using Soteo.Core.Shared.Dto.Snapshots;

namespace Soteo.Core.CampaignServer.Dto;

public sealed record CampaignSnapshot
{
    public required CampaignServerSnapshot CampaignServer { get; init; }
    public required IReadOnlyDictionary<Guid, ShardSnapshot> Shards { get; init; }
}
