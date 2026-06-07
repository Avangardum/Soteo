using Soteo.Core.CampaignServer.GameState.DataObjects;

namespace Soteo.Core.CampaignServer.Dto;

public sealed record CampaignServerSnapshot
{
    public required IReadOnlyDictionary<Guid, User> Users { get; init; }
    public required IReadOnlyDictionary<Guid, PlayerCharacter> Characters { get; init; }
}
