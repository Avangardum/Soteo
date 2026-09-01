using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Dto.Options;

public sealed record CampaignPersistenceOptions
{
    [Required]
    public required string SnapshotFolder { get; init; }
}
