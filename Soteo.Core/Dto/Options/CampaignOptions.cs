using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Dto.Options;

public sealed record CampaignOptions
{
    [Required]
    public required IReadOnlyList<Guid> ShardIds { get; init; }
}
