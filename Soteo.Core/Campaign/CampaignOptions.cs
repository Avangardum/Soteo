using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Campaign;

public sealed record CampaignOptions
{
    [Required]
    public required IReadOnlyList<Guid> ShardIds { get; init; }
}
