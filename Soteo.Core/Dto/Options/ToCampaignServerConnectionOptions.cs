using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Dto.Options;

public sealed record ToCampaignServerConnectionOptions
{
    [Required]
    public required string CampaignServerUrl { get; init; }
}
