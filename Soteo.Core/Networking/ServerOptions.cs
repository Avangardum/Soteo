using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Networking;

public sealed record ServerOptions
{
    [Required]
    public required string ServerSecret { get; init; }
}
