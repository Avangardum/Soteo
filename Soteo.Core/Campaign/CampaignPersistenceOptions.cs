using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Campaign;

public sealed record CampaignPersistenceOptions
{
    [Required]
    public required string SnapshotFolder { get; init; }
}
