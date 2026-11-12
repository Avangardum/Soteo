using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Dto.Options;

public sealed record ToAuthServerConnectionOptions
{
    [Required]
    public required string AuthServerUrl { get; init; }
}
