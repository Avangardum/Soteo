using Soteo.Core.Shared.Dto.Snapshots;

namespace Soteo.Core.CampaignServer.Dto.Snapshots;

public sealed record CampaignSnapshot
{
    public required CampaignServerSnapshot CampaignServer { get; init; }
    public required IReadOnlyDictionary<Guid, PersistenceShardSnapshot> Shards { get; init; }
}
