using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Networking;

public sealed record ToAuthServerConnectionOptions
{
    [Required]
    public required string AuthServerUrl { get; init; }
}
