using Soteo.Core.CampaignServer.GameState.DataObjects;

namespace Soteo.Core.CampaignServer.Dto.Snapshots;

public sealed record CampaignServerSnapshot
{
    public required IReadOnlyDictionary<Guid, UserSnapshot> Users { get; init; }
    public required IReadOnlyDictionary<Guid, PlayerCharacterSnapshot> Characters { get; init; }
}
