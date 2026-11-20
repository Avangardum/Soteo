namespace Soteo.Core.Snapshots;

public sealed record CampaignServerSnapshot
{
    public required IReadOnlyDictionary<Guid, UserSnapshot> Users { get; init; }
    public required IReadOnlyDictionary<Guid, PlayerCharacterTrackerSnapshot> PlayerCharacterTrackers { get; init; }
}
