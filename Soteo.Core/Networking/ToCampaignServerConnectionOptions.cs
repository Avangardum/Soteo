using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Networking;

public sealed record ToCampaignServerConnectionOptions
{
    [Required]
    public required string CampaignServerUrl { get; init; }
}
